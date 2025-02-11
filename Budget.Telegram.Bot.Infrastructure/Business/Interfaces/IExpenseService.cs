using Budget.Telegram.Bot.Domain.Entities;

namespace Budget.Telegram.Bot.Infrastructure.Business.Interfaces;

public interface IExpenseService
{
    Task<Expense?> FindById(long id);
    Task<bool> Update(Expense dbExpense, Expense expense);
    Task<bool> Delete(long id);
}