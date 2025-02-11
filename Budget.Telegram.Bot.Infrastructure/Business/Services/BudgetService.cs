using Budget.Telegram.Bot.Domain.Entities;
using Budget.Telegram.Bot.Infrastructure.Business.Interfaces;
using Budget.Telegram.Bot.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Budget.Telegram.Bot.Infrastructure.Business.Services;

public class BudgetService(ILogger<BudgetService> logger, AppDbContext dbContext) : IBudgetService
{
    public async Task<Domain.Entities.Budget?> FindById(long id)
    {
        try
        {
            var dbBudget = await dbContext.Budgets.FirstOrDefaultAsync(b => b.Id == id);

            return dbBudget;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }

    public async Task<List<Domain.Entities.Budget>> FindAllForUser(long userId)
    {
        try
        {
            var dbUser = await dbContext.TelegramUsers.Where(tu => tu.Id == userId).FirstOrDefaultAsync();
            if (dbUser == null)
                throw new NullReferenceException("User not found");
            
            return await dbContext.UsersGroups
                .Where(ug => ug.Users.Contains(dbUser))
                .SelectMany(ug => ug.Budgets)
                .ToListAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return new List<Domain.Entities.Budget>();
        }
    }

    public async Task<bool> Update(Domain.Entities.Budget budget)
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
            dbBudget.Amount -= expense.Amount;
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
        dbBudget.Amount += deposit.Amount;
        var res = await dbContext.SaveChangesAsync();
        return res > 0;
    }

    public async Task<List<Deposit>> GetDeposits(long id)
    {
        try
        {
            var dbBudget = await dbContext.Budgets
                .Include(b => b.Deposits)
                .FirstOrDefaultAsync(b => b.Id == id);
            
            return dbBudget == null ? [] : dbBudget.Deposits.ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return [];
        }
    }

    public async Task<List<TelegramUser>> GetUsersInBudget(long id)
    {
        try
        {
            var dbBudget = await dbContext.Budgets
                .Include(b => b.Groups)
                .ThenInclude(g => g.Users)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (dbBudget?.Groups == null || dbBudget.Groups.Count == 0)
                return [];

            var users = dbBudget.Groups
                .SelectMany(g => g.Users)
                .ToList();

            return users;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return [];
        }
    }
}