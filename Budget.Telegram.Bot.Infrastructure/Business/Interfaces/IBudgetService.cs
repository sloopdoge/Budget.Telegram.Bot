using Budget.Telegram.Bot.Domain.Entities;

namespace Budget.Telegram.Bot.Infrastructure.Business.Interfaces;

public interface IBudgetService
{
    Task<Domain.Entities.Budget?> FindById(long id);
    Task<List<Domain.Entities.Budget>> FindAllForUser(long userId);
    Task<bool> Update(Domain.Entities.Budget budget);
    Task<bool> Delete(long id);
    Task<bool> AddExpense(long id, Expense expense);
    Task<List<Expense>> GetExpenses(long id);
    Task<bool> AddDeposit(long id, Deposit deposit);
    Task<List<Deposit>> GetDeposits(long id);
    Task<List<TelegramUser>> GetUsersInBudget(long id);
}