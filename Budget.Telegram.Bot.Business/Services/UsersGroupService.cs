using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.DataAccess;
using Budget.Telegram.Bot.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Budget.Telegram.Bot.Business.Services;

public class UsersGroupService(ILogger<UsersGroupService> logger, AppDbContext dbContext) : IUsersGroupService
{
    public async Task<UsersGroup?> FindById(long id)
    {
        try
        {
            return await dbContext.UsersGroups.FindAsync(id);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }

    public async Task<bool> Update(UsersGroup dbGroup, UsersGroup group)
    {
        try
        {
            dbGroup.Update(group);

            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }

    public async Task<bool> Create(UsersGroup group)
    {
        try
        {
            dbContext.UsersGroups.Add(group);

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
            var dbGroup = await FindById(id);
            if (dbGroup == null)
                return false;
                
            dbContext.UsersGroups.Remove(dbGroup);

            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }

    public async Task<bool> AddUser(long groupId, TelegramUser user)
    {
        try
        {
            var dbGroup = await FindById(groupId);
            if (dbGroup == null)
                return false;
            
            dbGroup.Users.Add(user);
            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }

    public async Task<List<TelegramUser>> GetUsers(long id)
    {
        try
        {
            var dbGroup = await FindById(id);
            if (dbGroup == null)
                return [];

            return dbGroup.Users.ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return [];
        }
    }

    public async Task<bool> AddBudget(long groupId, Entity.Entities.Budget budget)
    {
        try
        {
            var dbGroup = await FindById(groupId);
            if (dbGroup == null)
                return false;
            
            dbGroup.Budgets.Add(budget);
            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }

    public async Task<List<Entity.Entities.Budget>> GetBudgets(long id)
    {
        try
        {
            var dbGroup = await FindById(id);
            if (dbGroup == null)
                return [];

            return dbGroup.Budgets.ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return [];
        }
    }
    
    public async Task<List<UsersGroup>> GetUserGroups(long userId)
    {
        return await dbContext.TelegramUsers
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Groups)
            .ToListAsync();
    }
}