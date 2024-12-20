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
            List<Entity.Entities.Budget> budgets = new List<Entity.Entities.Budget>();
            Entity.Entities.Budget chosenBudget;
            
            if (sessionStateService.GetCurrentMenuOrDefault(user.Id) is not MenuEnum.Budgets)
                sessionStateService.PushMenu(user.Id, MenuEnum.Budgets);

            if (sessionStateService.GetCurrentUserOperationOrDefault(user.Id) is UserOperationsEnum.ChoosingEditBudgetAction)
            {
                await ProcessEditBudgetAction(user, message, cancellationToken);
                return;
            }

            if (sessionStateService.GetCurrentUserOperationOrDefault(user.Id) is UserOperationsEnum.None)
            {
                budgets = await budgetService.FindAllForUser(user.Id);
                if (!budgets.Any())
                {
                    await botHelper.SendMessage(
                        user.ChatId, 
                        $"You don't have any budgets. You need to create at least one.",
                        cancellationToken: cancellationToken);
                    await menuManagementService.SetStartMenu(user, cancellationToken: cancellationToken);
                    return;
                }
            }

            switch (sessionStateService.GetCurrentUserOperationOrDefault(user.Id))
            {
                case UserOperationsEnum.ChoosingEditBudget:
                    if (!long.TryParse(message, out long budgetId)) throw new ArgumentException("Incorrect budget ID");
                    
                    var budget = await budgetService.FindById(budgetId);
                    if (budget == null)
                        throw new NullReferenceException("Budget not found");
                        
                    sessionStateService.PushUserChosenBudget(user.Id, budget);
                    
                    await SetChoosingEditActionMenu(user, cancellationToken);
                    break;
                case UserOperationsEnum.EditBudgetTitle:
                    chosenBudget = sessionStateService.GetUserChosenBudget(user.Id);

                    chosenBudget.Title = message;
                    sessionStateService.PushUserChosenBudget(user.Id, chosenBudget);
                    
                    await SetChoosingEditActionMenu(user, cancellationToken);
                    break;
                case UserOperationsEnum.EditBudgetDescription:
                    chosenBudget = sessionStateService.GetUserChosenBudget(user.Id);

                    chosenBudget.Description = message;
                    sessionStateService.PushUserChosenBudget(user.Id, chosenBudget);
                    
                    await SetChoosingEditActionMenu(user, cancellationToken);
                    break;
                case UserOperationsEnum.EditBudgetAmount:
                    chosenBudget = sessionStateService.GetUserChosenBudget(user.Id);

                    if (long.TryParse(message, out long budgetAmount))
                    {
                        chosenBudget.Amount = budgetAmount;
                        sessionStateService.PushUserChosenBudget(user.Id, chosenBudget);
                        await SetChoosingEditActionMenu(user, cancellationToken);
                        break;
                    }
                    
                    await botHelper.SendMessage(
                        user.ChatId, 
                        $"Please enter the number.",
                        cancellationToken: cancellationToken);
                    break;
                default:
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ChoosingEditBudget);
                    var inlineKeyboard = botHelper.BuildInlineKeyboard(budgets
                        .Select(b => new[] { InlineKeyboardButton.WithCallbackData(b.Title, b.Id.ToString()) }));
                    
                    await botHelper.SendMessage(
                        user.ChatId, 
                        $"Select a budget you want to edit:",
                        inlineKeyboard, 
                        cancellationToken);
                    break;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling EditBudget operation.");
            await botHelper.SendMessage(
                user.ChatId, 
                $"Something went wrong. Please try again later.", 
                cancellationToken: cancellationToken);
            await menuManagementService.SetBudgetMenu(user, cancellationToken);
        }
    }

    public async Task HandleListBudget(TelegramUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionStateService.GetCurrentMenuOrDefault(user.Id) is not MenuEnum.Budgets)
                sessionStateService.PushMenu(user.Id, MenuEnum.Budgets);

            var budgets = await budgetService.FindAllForUser(user.Id);
            
            var inlineKeyboard = botHelper.BuildInlineKeyboard(budgets
                .Select(b => new[] { InlineKeyboardButton.WithCallbackData($"{b.Title}: {b.Amount} \u20b4", b.Id.ToString()) }));
                    
            await botHelper.SendMessage(
                user.ChatId, 
                $"Here is your budgets:",
                inlineKeyboard, 
                cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling ListBudget operation.");
            await botHelper.SendMessage(
                user.ChatId, 
                $"Something went wrong. Please try again later.", 
                cancellationToken: cancellationToken);
            await menuManagementService.SetStartMenu(user, cancellationToken);
        }
    }

    public async Task AddNewExpense(TelegramUser user, string message = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionStateService.GetCurrentMenuOrDefault(user.Id) is not MenuEnum.AddExpense)
                sessionStateService.PushMenu(user.Id, MenuEnum.AddExpense);

            var budgets = await budgetService.FindAllForUser(user.Id);

            Entity.Entities.Budget? chosenBudget;
            switch (sessionStateService.GetCurrentUserOperationOrDefault(user.Id))
            {
                case UserOperationsEnum.None:
                    var inlineKeyboard = botHelper.BuildInlineKeyboard(budgets
                        .Select(b => new[] { InlineKeyboardButton.WithCallbackData(b.Title, b.Id.ToString()) }));
                    
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ListingBudgets);
                    
                    await botHelper.SendMessage(
                        user.ChatId, 
                        $"Choose budget you want to add expense:",
                        inlineKeyboard, 
                        cancellationToken);
                    break;
                case UserOperationsEnum.ListingBudgets:
                    if (!long.TryParse(message, out var chosenBudgetId))
                        throw new ArgumentException("Incorrect budget ID");
                    
                    chosenBudget = budgets.FirstOrDefault(b => b.Id == chosenBudgetId);
                    if (chosenBudget == null)
                        throw new NullReferenceException("Budget not found");
                    
                    sessionStateService.PushUserChosenBudget(user.Id, chosenBudget);
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.AddBudgetExpenseAmount);
                    
                    await botHelper.SendMessage(
                        user.ChatId,
                        $"Enter the amount of Expense:",
                        new ReplyKeyboardMarkup("Cancel"){ ResizeKeyboard = true },
                        cancellationToken
                        );
                    break;
                case UserOperationsEnum.AddBudgetExpenseAmount:
                    if (!double.TryParse(message, out double expenseAmount))
                        throw new ArgumentException("Incorrect expense amount value");
                    
                    sessionStateService.PushUserChosenExpense(user.Id, new Expense(){Amount = expenseAmount});
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.AddBudgetExpenseDescription);
                    
                    await botHelper.SendMessage(
                        user.ChatId,
                        $"Enter the description of Expense:",
                        new ReplyKeyboardMarkup("Cancel"){ ResizeKeyboard = true },
                        cancellationToken
                    );
                    break;
                case UserOperationsEnum.AddBudgetExpenseDescription:
                    chosenBudget = budgets.FirstOrDefault(b => b.Id == sessionStateService.GetUserChosenBudget(user.Id).Id);
                    if (chosenBudget == null)
                        throw new NullReferenceException("Budget not found");
                    
                    var chosenExpense = sessionStateService.GetUserChosenExpense(user.Id);
                    if (chosenExpense == null)
                        throw new NullReferenceException("Expense not found");

                    chosenExpense.Description = message;
                    sessionStateService.PushUserChosenExpense(user.Id, chosenExpense);
                    sessionStateService.ClearUserOperation(user.Id);
                    
                    var saveExpenseRes = await budgetService.AddExpense(chosenBudget.Id, chosenExpense);
                    await botHelper.SendMessage(
                        user.ChatId,
                        saveExpenseRes ? "Expense added" : "Expense add error",
                        cancellationToken: cancellationToken
                    );

                    if (saveExpenseRes)
                    {
                        chosenBudget = await budgetService.FindById(chosenBudget.Id);
                        var budgetUsers = await budgetService.GetUsersInBudget(chosenBudget.Id);
                        
                        foreach (var userToNotify in budgetUsers
                                     /*.Where(userToNotify => userToNotify.Id != user.Id)*/)
                        {
                            await botHelper.SendMessage(
                                userToNotify.ChatId,
                                $"Expense added to Budget: {chosenBudget.Title}\n" +
                                $"By User: {user.FirstName} {user.LastName} @{user.UserName}\n" +
                                $"Amount: {chosenExpense.Amount}\n",
                                cancellationToken: cancellationToken
                            );

                            await botHelper.SendMessage(
                                userToNotify.ChatId,
                                $"Budget amount: {chosenBudget.Amount}", 
                                cancellationToken: cancellationToken
                            );
                        }
                    }
                    
                    await menuManagementService.SetStartMenu(user, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException("Unknown operation.");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling AddNewExpense operation.");
            await botHelper.SendMessage(
                user.ChatId, 
                $"Something went wrong. Please try again later.", 
                cancellationToken: cancellationToken);
            await menuManagementService.SetStartMenu(user, cancellationToken);
        }
    }

    public async Task AddNewDeposit(TelegramUser user, string message = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionStateService.GetCurrentMenuOrDefault(user.Id) is not MenuEnum.AddDeposit)
                sessionStateService.PushMenu(user.Id, MenuEnum.AddDeposit);

            var budgets = await budgetService.FindAllForUser(user.Id);

            Entity.Entities.Budget? chosenBudget;
            switch (sessionStateService.GetCurrentUserOperationOrDefault(user.Id))
            {
                case UserOperationsEnum.None:
                    var inlineKeyboard = botHelper.BuildInlineKeyboard(budgets
                        .Select(b => new[] { InlineKeyboardButton.WithCallbackData(b.Title, b.Id.ToString()) }));
                    
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ListingBudgets);
                    
                    await botHelper.SendMessage(
                        user.ChatId, 
                        $"Choose budget you want to add deposit:",
                        inlineKeyboard, 
                        cancellationToken);
                    break;
                case UserOperationsEnum.ListingBudgets:
                    if (!long.TryParse(message, out var chosenBudgetId))
                        throw new ArgumentException("Incorrect budget ID");
                    
                    chosenBudget = budgets.FirstOrDefault(b => b.Id == chosenBudgetId);
                    if (chosenBudget == null)
                        throw new NullReferenceException("Budget not found");
                    
                    sessionStateService.PushUserChosenBudget(user.Id, chosenBudget);
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.AddBudgetDepositAmount);
                    
                    await botHelper.SendMessage(
                        user.ChatId,
                        $"Enter the amount of Deposit:",
                        new ReplyKeyboardMarkup("Cancel"){ ResizeKeyboard = true },
                        cancellationToken
                        );
                    break;
                case UserOperationsEnum.AddBudgetDepositAmount:
                    if (!double.TryParse(message, out double depositAmount))
                        throw new ArgumentException("Incorrect deposit amount value");
                    
                    sessionStateService.PushUserChosenDeposit(user.Id, new Deposit(){Amount = depositAmount});
                    sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.AddBudgetDepositDescription);
                    
                    await botHelper.SendMessage(
                        user.ChatId,
                        $"Enter the description of Deposit:",
                        new ReplyKeyboardMarkup("Cancel"){ ResizeKeyboard = true },
                        cancellationToken
                    );
                    break;
                case UserOperationsEnum.AddBudgetDepositDescription:
                    chosenBudget = budgets.FirstOrDefault(b => b.Id == sessionStateService.GetUserChosenBudget(user.Id).Id);
                    if (chosenBudget == null)
                        throw new NullReferenceException("Budget not found");
                    
                    var chosenDeposit = sessionStateService.GetUserChosenDeposit(user.Id);
                    if (chosenDeposit == null)
                        throw new NullReferenceException("Budget not found");

                    chosenDeposit.Description = message;
                    sessionStateService.PushUserChosenDeposit(user.Id, chosenDeposit);
                    sessionStateService.ClearUserOperation(user.Id);
                    
                    var saveExpenseRes = await budgetService.AddDeposit(chosenBudget.Id, chosenDeposit);
                    await botHelper.SendMessage(
                        user.ChatId,
                        saveExpenseRes ? "Deposit added" : "Deposit add error",
                        cancellationToken: cancellationToken
                    );
                    
                    if (saveExpenseRes)
                    {
                        chosenBudget = await budgetService.FindById(chosenBudget.Id);
                        var budgetUsers = await budgetService.GetUsersInBudget(chosenBudget.Id);
                        
                        foreach (var userToNotify in budgetUsers
                                     /*.Where(userToNotify => userToNotify.Id != user.Id)*/)
                        {
                            await botHelper.SendMessage(
                                userToNotify.ChatId,
                                $"Deposit added to Budget: {chosenBudget.Title}\n" +
                                $"By User: {user.FirstName} {user.LastName} @{user.UserName}\n" +
                                $"Amount: {chosenDeposit.Amount}\n",
                                cancellationToken: cancellationToken
                            );

                            await botHelper.SendMessage(
                                userToNotify.ChatId,
                                $"Budget amount: {chosenBudget.Amount}", 
                                cancellationToken: cancellationToken
                            );
                        }
                    }
                    
                    await menuManagementService.SetStartMenu(user, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException("Unknown operation.");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling AddNewDeposit operation.");
            await botHelper.SendMessage(
                user.ChatId, 
                $"Something went wrong. Please try again later.", 
                cancellationToken: cancellationToken);
            await menuManagementService.SetStartMenu(user, cancellationToken);
        }
    }

    private async Task ProcessEditBudgetAction(TelegramUser user, string message, CancellationToken cancellationToken = default)
    {
        switch (message)
        {
            case nameof(EditBudgetActionsEnum.EditTitle):
                sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.EditBudgetTitle);
                
                await botHelper.SendMessage(
                    user.ChatId, 
                    $"Send new budget title", 
                    new ReplyKeyboardMarkup("Cancel"), 
                    cancellationToken);
                return;
            case nameof(EditBudgetActionsEnum.EditDescription):
                sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.EditBudgetDescription);
                
                await botHelper.SendMessage(
                    user.ChatId, 
                    $"Send new budget description", 
                    new ReplyKeyboardMarkup("Cancel"), 
                    cancellationToken);
                return;
            case nameof(EditBudgetActionsEnum.EditAmount):
                sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.EditBudgetAmount);
                
                await botHelper.SendMessage(
                    user.ChatId, 
                    $"Send new budget amount", 
                    new ReplyKeyboardMarkup("Cancel"), 
                    cancellationToken);
                return;
            case nameof(EditBudgetActionsEnum.Save):
                sessionStateService.ClearUserOperation(user.Id);
                
                var chosenBudget = sessionStateService.GetUserChosenBudget(user.Id);
                var budgetSaveResult = await budgetService.Update(chosenBudget);
                if (!budgetSaveResult)
                    throw new Exception($"There was an error saving the budget.");
                
                await botHelper.SendMessage(user.ChatId, $"Budget updated", cancellationToken: cancellationToken);
                
                await menuManagementService.SetBudgetMenu(user, cancellationToken);
                return;
            default:
                await SetChoosingEditActionMenu(user, cancellationToken);
                break;
        }
    }

    private async Task SetChoosingEditActionMenu(TelegramUser user, CancellationToken cancellationToken = default)
    {
        sessionStateService.SetUserOperation(user.Id, UserOperationsEnum.ChoosingEditBudgetAction);
        
        var keyboard = new ReplyKeyboardMarkup(
            EditBudgetActionsEnum.EditTitle.ToString(),
            EditBudgetActionsEnum.EditDescription.ToString(),
            EditBudgetActionsEnum.EditAmount.ToString(),
            EditBudgetActionsEnum.Save.ToString()
        ){ResizeKeyboard = true};
                        
        await botHelper.SendMessage(
            user.ChatId, 
            $"Select the option from menu to edit in budget",
            keyboard, 
            cancellationToken: cancellationToken);
    }
}