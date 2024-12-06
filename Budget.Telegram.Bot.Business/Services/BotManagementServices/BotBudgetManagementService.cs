using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;
using Budget.Telegram.Bot.Business.Interfaces.Helpers;
using Budget.Telegram.Bot.Business.Services.Helpers;
using Budget.Telegram.Bot.Entity.Entities;
using Budget.Telegram.Bot.Entity.Enums;
using Budget.Telegram.Bot.Entity.Enums.Menus;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;

namespace Budget.Telegram.Bot.Business.Services.BotManagementServices;

public class BotBudgetManagementService(ILogger<BotBudgetManagementService> logger,
    IBotHelper botHelper,
    IBotSessionStateService sessionStateService,
    IUsersGroupService groupService,
    IBudgetService budgetService,
    IBotMenuManagementService menuManagementService) : IBotBudgetManagementService
{
    public async Task HandleAddBudget(TelegramUser user, string message = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionStateService.GetCurrentMenuOrDefault(user.Id) is not MenuEnum.Budgets)
                sessionStateService.PushMenu(user.Id, MenuEnum.Budgets);
            
            var userGroups = await groupService.GetUserGroups(user.Id);
            if (!userGroups.Any())
            {
                await botHelper.SendMessage(
                    user.ChatId, 
                    $"You don't have any groups. You need to create at least one group or join any group.", 
                    cancellationToken: cancellationToken);
                await menuManagementService.SetStartMenu(user, cancellationToken: cancellationToken);
                return;
            }

            Entity.Entities.Budget? chosenBudget;
            
            switch (sessionStateService.GetCurrentUserOperationOrDefault(user.Id))
            {
                case UserOperationsEnum.ChoosingGroupToAddBudget:
                    if (long.TryParse(message, out long groupId))
                    {
                        sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.AddBudgetTitle);
                    
                        sessionStateService.PushUserChosenBudget(user.Id, new Entity.Entities.Budget());

                        await botHelper.SendMessage(
                            user.ChatId, 
                            $"Send the title of the Budget", 
                            new ReplyKeyboardMarkup("Cancel"){ ResizeKeyboard = true }, 
                            cancellationToken: cancellationToken);
                        return;
                    }
                    
                    throw new ArgumentException("Incorrect group id");
                
                case UserOperationsEnum.AddBudgetTitle:
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.AddBudgetDescription);
                    
                    chosenBudget = sessionStateService.GetUserChosenBudget(user.Id);
                    
                    chosenBudget.Title = message;
                    sessionStateService.PushUserChosenBudget(user.Id, chosenBudget);
                    
                    await botHelper.SendMessage(
                        user.ChatId, 
                        $"Send the description of the Budget {chosenBudget.Title}", 
                        new ReplyKeyboardMarkup("Cancel"){ ResizeKeyboard = true },
                        cancellationToken: cancellationToken);
                    return;
                case UserOperationsEnum.AddBudgetDescription:
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.AddBudgetAmount);
                    
                    chosenBudget = sessionStateService.GetUserChosenBudget(user.Id);
                    
                    chosenBudget.Description = message;
                    sessionStateService.PushUserChosenBudget(user.Id, chosenBudget);
                    
                    var replyKeyboard = new ReplyKeyboardMarkup( new List<KeyboardButton>()
                        {
                            "Empty",
                            "Cancel"
                        });
                    
                    await botHelper.SendMessage(
                        user.ChatId, 
                        $"Send the amount you want to store in the budget {chosenBudget.Title}. Press 'Empty' to leave amount empty",
                        replyKeyboard,
                        cancellationToken: cancellationToken);
                    return;
                case UserOperationsEnum.AddBudgetAmount:
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.AddBudget);
                    
                    var chosenGroupId = sessionStateService.GetUserChosenGroup(user.Id);
                    var chosenGroup = await groupService.FindById(chosenGroupId);
                    if (chosenGroup == null)
                        throw new ArgumentException("Group not found");
                    
                    chosenBudget = sessionStateService.GetUserChosenBudget(user.Id);

                    long budgetAmount = 0;

                    long.TryParse(chosenBudget.Title, out budgetAmount);
                    
                    if (string.Equals(message, "Empty"))
                    {
                        budgetAmount = 0;
                    }
                    
                    chosenBudget.Amount = budgetAmount;

                    var budgetCreateResult = await groupService.AddBudget(chosenGroup.Id, chosenBudget);
                    if (budgetCreateResult)
                    {
                        await botHelper.SendMessage(
                            user.ChatId, 
                            $"Budget {chosenBudget.Title} added to {chosenGroup.Title}", 
                            cancellationToken: cancellationToken);
                        
                        sessionStateService.ClearUserOperation(user.Id);
                        await menuManagementService.SetBudgetMenu(user, cancellationToken);
                        return;
                    }
                    
                    throw new Exception("Budget was not added");
                default:
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ChoosingGroupToAddBudget);

                    var inlineKeyboard = botHelper.BuildInlineKeyboard(userGroups
                        .Select(g => new[] { InlineKeyboardButton.WithCallbackData(g.Title, g.Id.ToString()) }));
                    
                    await botHelper.SendMessage(
                        user.ChatId, 
                        $"Select a group to which add budget:",
                        inlineKeyboard, 
                        cancellationToken);
                    return;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling AddBudget operation.");
            sessionStateService.ClearUserOperation(user.Id);
            await botHelper.SendMessage(user.ChatId, "Something went wrong. Please try again later.", cancellationToken: cancellationToken);
            await menuManagementService.SetBudgetMenu(user, cancellationToken);
        }
    }

    public async Task HandleEditBudget(TelegramUser user, string message = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionStateService.GetCurrentMenuOrDefault(user.Id) is not MenuEnum.Budgets)
                sessionStateService.PushMenu(user.Id, MenuEnum.Budgets);
            
            var budgets = await budgetService.FindAllForUser(user.Id);
            if (!budgets.Any())
            {
                await botHelper.SendMessage(
                    user.ChatId, 
                    $"You don't have any budgets. You need to create at least one.",
                    cancellationToken: cancellationToken);
                await menuManagementService.SetStartMenu(user, cancellationToken: cancellationToken);
                return;
            }

            switch (sessionStateService.GetCurrentUserOperationOrDefault(user.Id))
            {
                default:
                    
                    break;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling EditBudget operation.");
            await botHelper.SendMessage(user.ChatId, "Something went wrong. Please try again later.", cancellationToken: cancellationToken);
            await menuManagementService.SetBudgetMenu(user, cancellationToken);
        }
    }

    public async Task HandleListBudget(TelegramUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling ListBudget operation.");
            await botHelper.SendMessage(user.ChatId, "Something went wrong. Please try again later.", cancellationToken: cancellationToken);
            await menuManagementService.SetBudgetMenu(user, cancellationToken);
        }
    }
}