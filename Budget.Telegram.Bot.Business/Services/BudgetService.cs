using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.DataAccess;
using Budget.Telegram.Bot.Entity.Entities;
using Microsoft.Extensions.Logging;

namespace Budget.Telegram.Bot.Business.Services;

public class BudgetService(ILogger<BudgetService> logger, AppDbContext dbContext, IUsersGroupService usersGroupService) : IBudgetService
{
    public async Task<Entity.Entities.Budget?> FindById(long id)
    {
        try
        {
            var dbBudget = await dbContext.Budgets.FindAsync(id);

            return dbBudget;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }

    public async Task<bool> Update(Entity.Entities.Budget budget)
    {
        try
        {
            var dbBudget = await FindById(budget.Id);
            if (dbBudget == null)
                return false;
            
            dbBudget.Update(budget);
            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }

    public async Task<bool> Delete(long id)
    {
        try
        {
            var dbBudget = await FindById(id);
            if (dbBudget == null)
                return false;

            dbContext.Budgets.Remove(dbBudget);
            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }

    public async Task<bool> AddExpense(long id, Expense expense)
    {
        try
        {
            var dbBudget = await FindById(id);
            if (dbBudget == null)
                return false;

            dbBudget.Expenses.Add(expense);
            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }

    public async Task<List<Expense>> GetExpenses(long id)
    {
        try
        {
            var dbBudget = await FindById(id);
            if (dbBudget == null)
                return [];

            return dbBudget.Expenses.ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return [];
        }
    }

    public async Task<bool> AddDeposit(long id, Deposit deposit)
    {
        var dbBudget = await FindById(id);
        if (dbBudget == null)
            return false;

        dbBudget.Deposits.Add(deposit);
        var res = await dbContext.SaveChangesAsync();
        return res > 0;
    }

    public async Task<List<Deposit>> GetDeposits(long id)
    {
        try
        {
            var dbBudget = await FindById(id);
            if (dbBudget == null)
                return [];

            return dbBudget.Deposits.ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return [];
        }
    }
}