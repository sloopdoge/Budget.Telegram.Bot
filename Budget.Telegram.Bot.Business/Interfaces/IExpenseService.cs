using Budget.Telegram.Bot.Entity.Entities;

namespace Budget.Telegram.Bot.Business.Interfaces;

public interface IExpenseService
{
    Task<Expense?> FindById(long id);
    Task<bool> Update(Expense dbExpense, Expense expense);
    Task<bool> Delete(long id);
}