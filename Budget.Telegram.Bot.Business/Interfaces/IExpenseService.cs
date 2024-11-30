using Budget.Telegram.Bot.Entity.Entities;

namespace Budget.Telegram.Bot.Business.Interfaces;

public interface IExpenseService
{
    Task<Expense> FindById(long id);
    Task<bool> Update(Expense budget);
    Task<bool> AddToBudget(Expense budget);
    Task<bool> Delete(long id);
}