using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;
using Budget.Telegram.Bot.Entity.Entities;
using Budget.Telegram.Bot.Entity.Enums.Menus;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Budget.Telegram.Bot.Business.Services.BotManagementServices;

public class BotGroupManagementService(
    ILogger<BotGroupManagementService> logger, 
    ITelegramBotClient botClient,
    ITelegramUserService telegramUserService,
    IBotSessionStateService botSessionStateService,
    IBotMenuManagementService botMenuManagementService,
    IUsersGroupService groupService) : IBotGroupManagementService
{
    public async Task HandleAddGroup(TelegramUser user, string groupTitle = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (botSessionStateService.GetCurrentMenu(user.Id) is MenuEnum.Groups 
                && botSessionStateService.GetCurrentUserOperation(user.Id) is UserOperationsEnum.AddGroup)
            {
                if (string.IsNullOrEmpty(groupTitle))
                {
                    await botClient.SendMessage(
                        chatId: user.ChatId,
                        text: "Group title cannot be empty. Please enter a valid group title.", 
                        cancellationToken: cancellationToken
                    );
                
                    return;
                }
                
                var newGroup = new UsersGroup()
                {
                    Title = groupTitle,
                    Users = new List<TelegramUser> { user }
                };
                
                var isCreated = await groupService.Create(newGroup);

                if (isCreated)
                {
                    await botClient.SendMessage(
                        chatId: user.ChatId,
                        text: $"Group {newGroup.Title} has been created", 
                        cancellationToken: cancellationToken
                    );
                    
                    botSessionStateService.ClearUserOperation(user.Id);
                    
                    return;
                }
                
                await botClient.SendMessage(
                    chatId: user.ChatId,
                    text: "Something went wrong. Try again.", 
                    cancellationToken: cancellationToken
                );
                
                botSessionStateService.ClearUserOperation(user.Id);
                await botMenuManagementService.SetGroupMenu(user, cancellationToken);
                
                return;
            }

            var keyboard = new KeyboardButton[]
            {
                "Cancel"
            };
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboard){ ResizeKeyboard = true };

            if (botSessionStateService.GetCurrentMenu(user.Id) is not MenuEnum.Groups)
                botSessionStateService.PushMenu(user.Id, MenuEnum.Groups);
            
            botSessionStateService.SetUserOperation(user.Id, UserOperationsEnum.AddGroup);

            await botClient.SendMessage(
                chatId: user.ChatId,
                text: "Enter the name of the group to add:",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken
                );
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            throw;
        }
    }

    public async Task HandleEditGroup(TelegramUser user, string message = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (botSessionStateService.GetCurrentMenu(user.Id) is MenuEnum.Groups)
            {
                UsersGroup? editGroup;
                
                switch (botSessionStateService.GetCurrentUserOperation(user.Id))
                {
                    case UserOperationsEnum.ChoosingEditGroup:
                        if (long.TryParse(message, out long editGroupId))
                        {
                            editGroup = await groupService.FindById(editGroupId);
                            if (editGroup == null)
                            {
                                await botClient.SendMessage(
                                    chatId: user.ChatId,
                                    text: "Group not found. Try again.", 
                                    cancellationToken: cancellationToken
                                );
                
                                botSessionStateService.ClearUserOperation(user.Id);
                                await botMenuManagementService.SetGroupMenu(user, cancellationToken);
                            }
                            
                            botSessionStateService.PushUserChosenGroup(user.Id, editGroup.Id);

                            if (botSessionStateService.GetCurrentMenu(user.Id) is not MenuEnum.Groups)
                                botSessionStateService.PushMenu(user.Id, MenuEnum.Groups);
                        
                            botSessionStateService.SetUserOperation(user.Id, UserOperationsEnum.EditGroup);

                            await botClient.SendMessage(
                                chatId: user.ChatId,
                                text: $"Enter new title of the group {editGroup?.Title}:",
                                cancellationToken: cancellationToken);
                        }
                        return;
                    case UserOperationsEnum.EditGroup:
                        editGroup = await groupService.FindById(botSessionStateService.GetUserChosenGroup(user.Id));
                        if (editGroup == null)
                        {
                            await botClient.SendMessage(
                                chatId: user.ChatId,
                                text: "Group not found. Try again.", 
                                cancellationToken: cancellationToken
                            );
                
                            botSessionStateService.ClearUserOperation(user.Id);
                            await botMenuManagementService.SetGroupMenu(user, cancellationToken);
                            
                            return;
                        }
                        
                        if (string.IsNullOrEmpty(message))
                        {
                            await botClient.SendMessage(
                                chatId: user.ChatId,
                                text: "Group title cannot be empty. Please enter a valid group title.", 
                                cancellationToken: cancellationToken
                            );
                
                            return;
                        }
                        
                        var newGroup = editGroup;
                        editGroup.Title = message;
                        
                        var isUpdated = await groupService.Update(editGroup, newGroup);
                        
                        if (isUpdated)
                        {
                            await botClient.SendMessage(
                                chatId: user.ChatId,
                                text: $"Group {newGroup.Title} has been updated", 
                                cancellationToken: cancellationToken
                            );
                    
                            botSessionStateService.ClearUserOperation(user.Id);
                            botSessionStateService.RemoveUserChosenGroup(user.Id);
                    
                            return;
                        }
                
                        await botClient.SendMessage(
                            chatId: user.ChatId,
                            text: "Something went wrong. Try again.", 
                            cancellationToken: cancellationToken
                        );
                
                        botSessionStateService.ClearUserOperation(user.Id);
                        await botMenuManagementService.SetGroupMenu(user, cancellationToken);
                        
                        return;
                    default:
                        if (botSessionStateService.GetCurrentMenu(user.Id) is not MenuEnum.Groups)
                            botSessionStateService.PushMenu(user.Id, MenuEnum.Groups);
                        
                        botSessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ChoosingEditGroup);

                        var userGroups = await groupService.GetUserGroups(user.Id);
                        
                        var inlineKeyboardButtons = userGroups
                            .Select(userGroup => InlineKeyboardButton.WithCallbackData(userGroup.Title, userGroup.Id.ToString()))
                            .Chunk(1)
                            .Select(chunk => chunk.ToArray())
                            .ToList();

                        var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);

                        await botClient.SendMessage(
                            chatId: user.ChatId,
                            text: $"Group lists:",
                            replyMarkup: inlineKeyboard, 
                            cancellationToken: cancellationToken
                            );
                        
                        return;
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            throw;
        }
    }

    public async Task HandleInviteGroup(TelegramUser user, string message = "", UpdateType messageType = UpdateType.Message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (botSessionStateService.GetCurrentMenu(user.Id) is not MenuEnum.Groups)
            {
                botSessionStateService.PushMenu(user.Id, MenuEnum.Groups);
            }
            
            if (botSessionStateService.GetCurrentMenu(user.Id) is MenuEnum.Groups && 
                (botSessionStateService.GetCurrentUserOperation(user.Id) is not UserOperationsEnum.ChoosingInviteGroup and not UserOperationsEnum.InviteToGroup || 
                 botSessionStateService.GetCurrentUserOperation(user.Id) == null))
            {
                botSessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ChoosingInviteGroup);
                
                var userGroups = await groupService.GetUserGroups(user.Id);

                var inlineKeyboardButtons = userGroups
                    .Select(userGroup =>
                        InlineKeyboardButton.WithCallbackData(userGroup.Title, userGroup.Id.ToString()))
                    .Chunk(1)
                    .Select(chunk => chunk.ToArray())
                    .ToList();

                var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);

                await botClient.SendMessage(
                    chatId: user.ChatId,
                    text: $"Inviting to group",
                    replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton("Cancel")){ResizeKeyboard = true},
                    cancellationToken: cancellationToken
                );
                await botClient.SendMessage(
                    chatId: user.ChatId,
                    text: $"Select a group in which you want to invite:",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken
                );
                
                
                return;
            }

            if (botSessionStateService.GetCurrentMenu(user.Id) is MenuEnum.Groups  &&
                botSessionStateService.GetCurrentUserOperation(user.Id) is UserOperationsEnum.ChoosingInviteGroup)
            {
                botSessionStateService.SetUserOperation(user.Id, UserOperationsEnum.InviteToGroup);

                if (long.TryParse(message, out long groupId))
                {
                    var chosenGroup = await groupService.FindById(groupId);
                    if (chosenGroup == null)
                    {
                        await botClient.SendMessage(
                            chatId: user.ChatId,
                            text: "Group not found. Try again.", 
                            cancellationToken: cancellationToken
                        );
                
                        botSessionStateService.ClearUserOperation(user.Id);
                        await botMenuManagementService.SetGroupMenu(user, cancellationToken);
                        
                        return;
                    }
                    
                    botSessionStateService.PushUserChosenGroup(user.Id, chosenGroup.Id);
                    
                    await botClient.SendMessage(
                        chatId: user.ChatId,
                        text: $"Send username of user you want to invite to group {chosenGroup.Title}:",
                        cancellationToken: cancellationToken
                        );
                    
                    return;
                }
                return;
            }

            if (botSessionStateService.GetCurrentMenu(user.Id) is MenuEnum.Groups &&
                botSessionStateService.GetCurrentUserOperation(user.Id) is UserOperationsEnum.InviteToGroup)
            {
                var chosenGroup = await groupService.FindById(botSessionStateService.GetUserChosenGroup(user.Id));
                if (chosenGroup == null)
                {
                    await botClient.SendMessage(
                        chatId: user.ChatId,
                        text: "Group not found. Try again.", 
                        cancellationToken: cancellationToken
                    );
                
                    botSessionStateService.ClearUserOperation(user.Id);
                    await botMenuManagementService.SetGroupMenu(user, cancellationToken);
                        
                    return;
                }

                var username = message.Trim('@',' ');
                if (string.IsNullOrEmpty(username))
                {
                    await botClient.SendMessage(
                        chatId: user.ChatId,
                        text: "Username not correct. Try again.", 
                        cancellationToken: cancellationToken
                    );
                        
                    return;
                }
                
                var userToInvite = await telegramUserService.FindByUsername(username);
                if (userToInvite == null)
                {
                    await botClient.SendMessage(
                        chatId: user.ChatId,
                        text: "User not found. This user need's to authorize with me.",
                        cancellationToken: cancellationToken
                    );
                      
                    botSessionStateService.ClearUserOperation(user.Id);
                    await botMenuManagementService.SetGroupMenu(user, cancellationToken);
                    
                    return;
                }

                var inviteResult = await groupService.AddUser(chosenGroup.Id, userToInvite);
                if (inviteResult)
                {
                    await botClient.SendMessage(
                        chatId: user.ChatId,
                        text: $"User successfully added to group {chosenGroup.Title}.",
                        cancellationToken: cancellationToken
                    );

                    await botClient.SendMessage(
                        chatId: userToInvite.ChatId,
                        text: $"You have been invited to the group {chosenGroup.Title} by {user.FirstName} {user.LastName}",
                        cancellationToken: cancellationToken
                    );
                      
                    botSessionStateService.ClearUserOperation(user.Id);
                    await botMenuManagementService.SetGroupMenu(user, cancellationToken);
                    
                    return;
                }
                
                await botClient.SendMessage(
                    chatId: user.ChatId,
                    text: "Something went wrong. Try again.",
                    cancellationToken: cancellationToken
                );
                      
                botSessionStateService.ClearUserOperation(user.Id);
                await botMenuManagementService.SetGroupMenu(user, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            throw;
        }
    }

    public async Task HandleListGroups(TelegramUser user, string groupTitle = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (botSessionStateService.GetCurrentMenu(user.Id) is not MenuEnum.Groups &&
                botSessionStateService.GetCurrentUserOperation(user.Id) is not UserOperationsEnum.ListingGroups)
            {
                botSessionStateService.PushMenu(user.Id, MenuEnum.Groups);
                botSessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ListingGroups);

                var userGroups = await groupService.GetUserGroups(user.Id);

                var inlineKeyboardButtons = userGroups
                    .Select(userGroup =>
                        InlineKeyboardButton.WithCallbackData(userGroup.Title, userGroup.Id.ToString()))
                    .Chunk(1)
                    .Select(chunk => chunk.ToArray())
                    .ToList();

                var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);

                await botClient.SendMessage(
                    chatId: user.ChatId,
                    text: $"List of your groups:",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken
                );

                return;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            throw;
        }
    }
}