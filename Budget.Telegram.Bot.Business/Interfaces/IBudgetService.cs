using Budget.Telegram.Bot.Entity.Entities;

namespace Budget.Telegram.Bot.Business.Interfaces;

public interface IBudgetService
{
    Task<Entity.Entities.Budget?> FindById(long id);
    Task<bool> Update(Entity.Entities.Budget budget);
    Task<bool> Delete(long id);
    Task<bool> AddExpense(long id, Expense expense);
    Task<List<Expense>> GetExpenses(long id);
    Task<bool> AddDeposit(long id, Deposit deposit);
    Task<List<Deposit>> GetDeposits(long id);
}