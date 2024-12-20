using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;
using Budget.Telegram.Bot.Business.Services.BotManagementServices;
using Budget.Telegram.Bot.Entity.Entities;
using Budget.Telegram.Bot.Entity.Enums;
using Budget.Telegram.Bot.Entity.Enums.Menus;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Budget.Telegram.Bot.Business.Services;

public class BotHandler(
    ILogger<BotHandler> logger, 
    ITelegramBotClient botClient,
    ITelegramUserService userService, 
    IBotMenuManagementService menuManagementService, 
    IBotSessionStateService sessionStateService,
    IBotGroupManagementService groupManagementService,
    IBotBudgetManagementService budgetManagementService)
{
    private TelegramUser? _currentUser;
    private MenuEnum? _currentMainMenu;
    private UserOperationsEnum? _currentUserOperation;
    
    public async Task HandleUpdate(Update update)
    {
        try
        {
            if (update == null)
                throw new ArgumentNullException(nameof(update), "Update cannot be null.");

            await InitCurrentUser(update);
            InitCurrentMenu();
            InitCurrentOperation();

            if (_currentUser == null)
                throw new InvalidOperationException("User not found during update handling.");

            if (HandleStartCommand(update.Message?.Text))
                return;

            await HandleMenuActions(update);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in HandleUpdate.");
            await botClient.SendMessage(_currentUser?.ChatId ?? 0, "An error occurred. Please try again.");
        }
    }

    private async Task HandleMenuActions(Update update)
    {
        if (string.Equals(update.Message?.Text, MenuEnum.Back.ToString()))
        {
            await menuManagementService.HandleGoBack(_currentUser);
            return;
        }
        /*
        if (_currentMainMenu is MenuEnum.Groups && _currentUserOperation is not UserOperationsEnum.None)
        {
            await HandleGroupMenuActions(update);
            return;
        }
        
        if (_currentMainMenu is MenuEnum.Budgets && _currentUserOperation is not UserOperationsEnum.None)
        {
            await HandleBudgetMenuActions(update);
            return;
        }
        */
        
        if (_currentMainMenu.HasValue && new List<MenuEnum>
                                      {
                                          MenuEnum.Groups, 
                                          MenuEnum.Budgets, 
                                          MenuEnum.AddDeposit, 
                                          MenuEnum.AddExpense
                                      }.Contains(_currentMainMenu.Value) 
            && _currentUserOperation is not UserOperationsEnum.None)
        {
            switch (_currentMainMenu)
            {
                case MenuEnum.Groups:
                    await HandleGroupMenuActions(update);
                    return;
                case MenuEnum.Budgets:
                    await HandleBudgetMenuActions(update);
                    return;
                case MenuEnum.AddDeposit:
                    await budgetManagementService.AddNewDeposit(_currentUser,
                        update.Type is UpdateType.CallbackQuery ? update.CallbackQuery.Data : update.Message?.Text);
                    return;
                case MenuEnum.AddExpense:
                    await budgetManagementService.AddNewExpense(_currentUser,
                        update.Type is UpdateType.CallbackQuery ? update.CallbackQuery.Data : update.Message?.Text);
                    return;
            }
        }

        switch (update.Message?.Text)
        {
            case nameof(StartMenuEnum.Groups):
                await menuManagementService.SetGroupMenu(_currentUser);
                break;
            case nameof(StartMenuEnum.Budgets):
                await menuManagementService.SetBudgetMenu(_currentUser);
                break;
            case nameof(GroupMenuEnum.AddGroup):
                await groupManagementService.HandleAddGroup(_currentUser);
                break;
            case nameof(GroupMenuEnum.EditGroup):
                await groupManagementService.HandleEditGroup(_currentUser);
                break;
            case nameof(GroupMenuEnum.InviteToGroup):
                await groupManagementService.HandleInviteGroup(_currentUser);
                break;
            case nameof(GroupMenuEnum.ListMyGroups):
                await groupManagementService.HandleListGroups(_currentUser);
                break;
            case nameof(BudgetMenuEnum.AddBudget):
                await budgetManagementService.HandleAddBudget(_currentUser);
                break;
            case nameof(BudgetMenuEnum.EditBudget):
                await budgetManagementService.HandleEditBudget(_currentUser);
                break;
            case nameof(BudgetMenuEnum.ListMyBudgets):
                await budgetManagementService.HandleListBudget(_currentUser);
                break;
            case nameof(StartMenuEnum.AddExpense):
                await budgetManagementService.AddNewExpense(_currentUser);
                break;
            case nameof(StartMenuEnum.AddDeposit):
                await budgetManagementService.AddNewDeposit(_currentUser);
                break;
            default:
                logger.LogWarning($"Unhandled command: {update.Message?.Text}");
                await botClient.SendMessage(_currentUser.ChatId, "Unknown command. Please try again.");
                await menuManagementService.SetStartMenu(_currentUser);
                break;
        }
    }

    private async Task HandleBudgetMenuActions(Update update)
    {
        if (_currentUserOperation == null)
        {
            logger.LogWarning("No operation set for the current user.");
            return;
        }

        switch (_currentUserOperation)
        {
            case UserOperationsEnum.AddBudget: 
            case UserOperationsEnum.AddBudgetTitle: 
            case UserOperationsEnum.AddBudgetDescription: 
            case UserOperationsEnum.AddBudgetAmount:
                await budgetManagementService.HandleAddBudget(_currentUser, update.Message.Text);
                break;
            case UserOperationsEnum.ChoosingGroupToAddBudget:
                await budgetManagementService.HandleAddBudget(_currentUser, update.CallbackQuery.Data);
                break;
            case UserOperationsEnum.EditBudgetTitle:
            case UserOperationsEnum.EditBudgetAmount:
            case UserOperationsEnum.EditBudgetDescription:
                await budgetManagementService.HandleEditBudget(_currentUser, update.Message.Text);
                break;
            case UserOperationsEnum.ChoosingEditBudget:
                await budgetManagementService.HandleEditBudget(_currentUser, update.CallbackQuery.Data);
                break;
            case UserOperationsEnum.ChoosingEditBudgetAction:
                await budgetManagementService.HandleEditBudget(_currentUser, update.Message.Text);
                break;
            default:
                logger.LogWarning($"Unhandled operation: {_currentUserOperation}");
                break;
        }
    }

    private async Task HandleGroupMenuActions(Update update)
    {
        if (_currentUserOperation == null)
        {
            logger.LogWarning("No operation set for the current user.");
            return;
        }

        switch (_currentUserOperation)
        {
            case UserOperationsEnum.AddGroup:
                await HandleUserOperation(update, groupManagementService.HandleAddGroup);
                break;
            case UserOperationsEnum.EditGroup:
            case UserOperationsEnum.ChoosingEditGroup:
                await HandleUserOperation(update, groupManagementService.HandleEditGroup);
                break;
            case UserOperationsEnum.InviteToGroup:
            case UserOperationsEnum.ChoosingInviteGroup:
                await HandleUserOperation(update, groupManagementService.HandleInviteGroup);
                break;
            default:
                logger.LogWarning($"Unhandled operation: {_currentUserOperation}");
                break;
        }
    }

    private async Task HandleUserOperation(Update update, Func<TelegramUser, string, CancellationToken, Task> operationHandler)
    {
        var messageText = update.Message?.Text ?? update.CallbackQuery?.Data ?? string.Empty;

        if (string.Equals(messageText, "Cancel", StringComparison.OrdinalIgnoreCase))
        {
            sessionStateService.ClearUserOperation(_currentUser.Id);
            await menuManagementService.SetGroupMenu(_currentUser);
            return;
        }

        await operationHandler(_currentUser, messageText, default);
    }

    private bool HandleStartCommand(string? messageText)
    {
        if (!string.Equals("/start", messageText, StringComparison.OrdinalIgnoreCase))
            return false;

        menuManagementService.SetStartMenu(_currentUser);
        return true;
    }

    private void InitCurrentMenu() =>
        _currentMainMenu = sessionStateService.GetCurrentMenuOrDefault(_currentUser.Id);

    private void InitCurrentOperation() =>
        _currentUserOperation = sessionStateService.GetCurrentUserOperationOrDefault(_currentUser.Id);

    private async Task InitCurrentUser(Update update)
    {
        var user = update.Message?.From ?? update.CallbackQuery?.From;

        if (user == null)
            throw new ArgumentNullException("User information is missing in the update.");

        if (!await userService.CheckIfExist(new TelegramUser
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.Username,
            LanguageCode = user.LanguageCode,
            ChatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id ?? 0
        }))
        {
            throw new InvalidOperationException("User not found or unauthorized.");
        }

        _currentUser = await userService.FindById(user.Id);
    }

}