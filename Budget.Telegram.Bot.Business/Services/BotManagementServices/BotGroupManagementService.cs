using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;
using Budget.Telegram.Bot.Entity.Entities;
using Budget.Telegram.Bot.Entity.Enums.Menus;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Budget.Telegram.Bot.Business.Services.BotManagementServices;

public class BotGroupManagementService(
    ILogger<BotGroupManagementService> logger, 
    ITelegramBotClient botClient, 
    IBotSessionStateService botSessionStateService,
    IBotMenuManagementService botMenuManagementService,
    IUsersGroupService groupService) : IBotGroupManagementService
{
    private Dictionary<long, long> _chosenGroups = new Dictionary<long, long>();
    
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
                        if (int.TryParse(message, out int editGroupId))
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
                            
                            _chosenGroups.Add(user.Id, editGroup.Id);

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
                        editGroup = await groupService.FindById(_chosenGroups[user.Id]);
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

    public async Task HandleInviteGroup(TelegramUser user, string username = "", CancellationToken cancellationToken = default)
    {
        try
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            throw;
        }
    }
}