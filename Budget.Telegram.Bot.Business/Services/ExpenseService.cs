using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.DataAccess;
using Budget.Telegram.Bot.Entity.Entities;
using Microsoft.Extensions.Logging;

namespace Budget.Telegram.Bot.Business.Services;

public class ExpenseService(ILogger<ExpenseService> logger, AppDbContext dbContext) : IExpenseService
{
    public async Task<Expense?> FindById(long id)
    {
        try
        {
            var dbExpense = await dbContext.Expenses.FindAsync(id);
            return dbExpense;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            return null;
        }
    }

    public async Task<bool> Update(Expense dbExpense, Expense expense)
    {
        try
        {
            dbExpense.Update(expense);
            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            return false;
        }
    }

    public async Task<bool> Delete(long id)
    {
        try
        {
            var dbExpense = await FindById(id);
            if (dbExpense == null)
                return false;

            dbContext.Expenses.Remove(dbExpense);
            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            return false;
        }
    }
}