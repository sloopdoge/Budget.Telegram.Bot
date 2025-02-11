using Budget.Telegram.Bot.Domain.Entities;
using Budget.Telegram.Bot.Domain.Enums;
using Budget.Telegram.Bot.Domain.Enums.Menus;

namespace Budget.Telegram.Bot.Infrastructure.Business.Interfaces;

public interface IBotSessionStateService
{
    void PushMenu(long userId, MenuEnum menu);
    MenuEnum? PopMenu(long userId);
    MenuEnum GetCurrentMenuOrDefault(long userId);
    void SetUserOperation(long userId, UserOperationsEnum operation);
    UserOperationsEnum GetCurrentUserOperationOrDefault(long userId);
    void ClearUserOperation(long userId);
    void PushUserChosenGroup(long userId, long groupId);
    long GetUserChosenGroup(long userId);
    void RemoveUserChosenGroup(long userId);
    void PushUserChosenBudget(long userId, Domain.Entities.Budget budget);
    Domain.Entities.Budget GetUserChosenBudget(long userId);
    void PushUserChosenExpense(long userId, Expense expense);
    Expense GetUserChosenExpense(long userId);
    void PushUserChosenDeposit(long userId, Deposit deposit);
    Deposit GetUserChosenDeposit(long userId);
}