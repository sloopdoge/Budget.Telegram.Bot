using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;
using Budget.Telegram.Bot.Business.Interfaces.Helpers;
using Budget.Telegram.Bot.Entity.Entities;
using Budget.Telegram.Bot.Entity.Enums.Menus;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;

namespace Budget.Telegram.Bot.Business.Services.BotManagementServices;

public class BotGroupManagementService(
    ILogger<BotGroupManagementService> logger,
    ITelegramUserService userService,
    IBotSessionStateService sessionStateService,
    IBotMenuManagementService menuManagementService,
    IUsersGroupService groupService,
    IBotHelper botHelper) : IBotGroupManagementService
{
    public async Task HandleAddGroup(TelegramUser user, string groupTitle = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionStateService.GetCurrentUserOperationOrDefault(user.Id) != UserOperationsEnum.AddGroup)
            {
                sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.AddGroup);
                await botHelper.SendMessage(user.ChatId, "Enter the group name:", new ReplyKeyboardMarkup("Cancel"), cancellationToken);
                return;
            }

            if (string.IsNullOrEmpty(groupTitle))
            {
                await botHelper.SendMessage(user.ChatId, "Group title cannot be empty. Try again.", cancellationToken: cancellationToken);
                return;
            }

            var isCreated = await groupService.Create(new UsersGroup { Title = groupTitle, Users = new List<TelegramUser> { user } });

            var response = isCreated
                ? $"Group '{groupTitle}' created successfully."
                : "Failed to create group. Try again.";
            await botHelper.SendMessage(user.ChatId, response, cancellationToken: cancellationToken);

            sessionStateService.ClearUserOperation(user.Id);
            await menuManagementService.SetGroupMenu(user, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling AddGroup operation.");
            await botHelper.SendMessage(user.ChatId, "Something went wrong. Please try again later.", cancellationToken: cancellationToken);
            await menuManagementService.SetGroupMenu(user, cancellationToken);
        }
    }

    public async Task HandleEditGroup(TelegramUser user, string message = "", CancellationToken cancellationToken = default)
    {
        try
        {
            switch (sessionStateService.GetCurrentUserOperationOrDefault(user.Id))
            {
                case UserOperationsEnum.ChoosingEditGroup:
                    if (long.TryParse(message, out var groupId))
                    {
                        var group = await groupService.FindById(groupId);
                        if (group == null)
                        {
                            await botHelper.SendMessage(user.ChatId, "Group not found. Try again.", cancellationToken: cancellationToken);
                            sessionStateService.ClearUserOperation(user.Id);
                            await menuManagementService.SetGroupMenu(user, cancellationToken);
                            return;
                        }

                        sessionStateService.PushUserChosenGroup(user.Id, groupId);
                        sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.EditGroup);

                        await botHelper.SendMessage(user.ChatId, $"Enter new title for the group '{group.Title}':", new ReplyKeyboardMarkup("Cancel"), cancellationToken: cancellationToken);
                        return;
                    }

                    await botHelper.SendMessage(user.ChatId, "Invalid group selection. Try again.", cancellationToken: cancellationToken);
                    break;

                case UserOperationsEnum.EditGroup:
                    var editGroup = await groupService.FindById(sessionStateService.GetUserChosenGroup(user.Id));
                    if (editGroup == null)
                    {
                        await botHelper.SendMessage(user.ChatId, "Group not found. Try again.", cancellationToken: cancellationToken);
                        sessionStateService.ClearUserOperation(user.Id);
                        await menuManagementService.SetGroupMenu(user, cancellationToken);
                        return;
                    }

                    if (string.IsNullOrEmpty(message))
                    {
                        await botHelper.SendMessage(user.ChatId, "Group title cannot be empty. Try again.", new ReplyKeyboardMarkup("Cancel"), cancellationToken: cancellationToken);
                        return;
                    }

                    var editedGroup = editGroup;
                    editedGroup.Title = message;
                    var isUpdated = await groupService.Update(editGroup, editedGroup);

                    var updateResponse = isUpdated
                        ? $"Group '{editGroup.Title}' updated successfully."
                        : "Failed to update the group. Try again.";
                    await botHelper.SendMessage(user.ChatId, updateResponse, cancellationToken: cancellationToken);

                    sessionStateService.ClearUserOperation(user.Id);
                    sessionStateService.RemoveUserChosenGroup(user.Id);
                    await menuManagementService.SetGroupMenu(user, cancellationToken);
                    break;

                default:
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ChoosingEditGroup);

                    var userGroups = await groupService.GetUserGroups(user.Id);
                    var inlineKeyboard = botHelper.BuildInlineKeyboard(userGroups
                        .Select(g => new[] { InlineKeyboardButton.WithCallbackData(g.Title, g.Id.ToString()) }));

                    await botHelper.SendMessage(user.ChatId, "Select a group to edit:", inlineKeyboard, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling EditGroup operation.");
            await botHelper.SendMessage(user.ChatId, "Something went wrong. Please try again later.", cancellationToken: cancellationToken);
            await menuManagementService.SetGroupMenu(user, cancellationToken);
        }
    }

    public async Task HandleInviteGroup(TelegramUser user, string message = "", CancellationToken cancellationToken = default)
    {
        try
        {
            var currentOperation = sessionStateService.GetCurrentUserOperationOrDefault(user.Id);

            if (currentOperation != UserOperationsEnum.ChoosingInviteGroup && currentOperation != UserOperationsEnum.InviteToGroup)
            {
                sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ChoosingInviteGroup);

                var userGroups = await groupService.GetUserGroups(user.Id);
                if (!userGroups.Any())
                {
                    await botHelper.SendMessage(user.ChatId, "You have no groups to invite users to.", cancellationToken: cancellationToken);
                    await menuManagementService.SetGroupMenu(user, cancellationToken);
                    return;
                }

                var inlineKeyboard = botHelper.BuildInlineKeyboard(userGroups
                    .Select(g => new[] { InlineKeyboardButton.WithCallbackData(g.Title, g.Id.ToString()) }));

                await botHelper.SendMessage(user.ChatId, "Invite to group", new ReplyKeyboardMarkup("Cancel"), cancellationToken);
                await botHelper.SendMessage(user.ChatId, "Select a group to invite a user:", inlineKeyboard, cancellationToken);
                return;
            }

            if (currentOperation == UserOperationsEnum.ChoosingInviteGroup)
            {
                if (long.TryParse(message, out var groupId))
                {
                    var chosenGroup = await groupService.FindById(groupId);
                    if (chosenGroup == null)
                    {
                        await botHelper.SendMessage(user.ChatId, "Group not found. Try again.", cancellationToken: cancellationToken);
                        await menuManagementService.SetGroupMenu(user, cancellationToken);
                        return;
                    }

                    sessionStateService.PushUserChosenGroup(user.Id, groupId);
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.InviteToGroup);

                    await botHelper.SendMessage(user.ChatId, $"Enter the username to invite to '{chosenGroup.Title}':", new ReplyKeyboardMarkup("Cancel"), cancellationToken: cancellationToken);
                    return;
                }

                await botHelper.SendMessage(user.ChatId, "Invalid group selection. Please try again.", cancellationToken: cancellationToken);
                return;
            }

            if (currentOperation == UserOperationsEnum.InviteToGroup)
            {
                var groupId = sessionStateService.GetUserChosenGroup(user.Id);
                var chosenGroup = await groupService.FindById(groupId);
                if (chosenGroup == null)
                {
                    await botHelper.SendMessage(user.ChatId, "Group not found. Please try again.", cancellationToken: cancellationToken);
                    await menuManagementService.SetGroupMenu(user, cancellationToken);
                    return;
                }

                var username = message.Trim('@', ' ');
                if (string.IsNullOrEmpty(username))
                {
                    await botHelper.SendMessage(user.ChatId, "Invalid username. Please try again.", cancellationToken: cancellationToken);
                    return;
                }

                var userToInvite = await userService.FindByUsername(username);
                if (userToInvite == null)
                {
                    await botHelper.SendMessage(user.ChatId, "User not found or they have not authorized this bot. Please try again.", cancellationToken: cancellationToken);
                    await menuManagementService.SetGroupMenu(user, cancellationToken);
                    return;
                }

                var inviteResult = await groupService.AddUser(groupId, userToInvite);
                var inviteMessage = inviteResult
                    ? $"User '{username}' successfully invited to '{chosenGroup.Title}'."
                    : "Failed to invite user. Please try again.";

                await botHelper.SendMessage(user.ChatId, inviteMessage, cancellationToken: cancellationToken);

                if (inviteResult)
                {
                    await botHelper.SendMessage(userToInvite.ChatId, 
                        $"You have been invited to the group '{chosenGroup.Title}' by {user.FirstName} {user.LastName}.", 
                        cancellationToken: cancellationToken);
                }

                sessionStateService.ClearUserOperation(user.Id);
                await menuManagementService.SetGroupMenu(user, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling InviteGroup operation.");
            await botHelper.SendMessage(user.ChatId, "Something went wrong. Please try again later.", cancellationToken: cancellationToken);
            await menuManagementService.SetGroupMenu(user, cancellationToken);
        }
    }
    
    public async Task HandleListGroups(TelegramUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionStateService.GetCurrentUserOperationOrDefault(user.Id) != UserOperationsEnum.ListingGroups)
            {
                sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ListingGroups);
                sessionStateService.PushMenu(user.Id, MenuEnum.Groups);
            }

            var userGroups = await groupService.GetUserGroups(user.Id);
            if (!userGroups.Any())
            {
                await botHelper.SendMessage(user.ChatId, "You have no groups.", cancellationToken: cancellationToken);
                await menuManagementService.SetGroupMenu(user, cancellationToken);
                return;
            }

            var inlineKeyboard = botHelper.BuildInlineKeyboard(userGroups
                .Select(group => new[] { InlineKeyboardButton.WithCallbackData(group.Title, group.Id.ToString()) }));

            await botHelper.SendMessage(user.ChatId, "Here are your groups:", inlineKeyboard, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ListGroups operation.");
            await botHelper.SendMessage(user.ChatId, "Something went wrong. Please try again later.", cancellationToken: cancellationToken);
            await menuManagementService.SetGroupMenu(user, cancellationToken);
        }
    }
}