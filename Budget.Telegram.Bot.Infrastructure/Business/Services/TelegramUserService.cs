using Budget.Telegram.Bot.Domain.Entities;
using Budget.Telegram.Bot.Infrastructure.Business.Interfaces;
using Budget.Telegram.Bot.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Budget.Telegram.Bot.Infrastructure.Business.Services;

public class TelegramUserService(ILogger<TelegramUserService> logger, AppDbContext dbContext) : ITelegramUserService
{
    public async Task<TelegramUser?> FindById(long id)
    {
        try
        {
            var dbUser = await dbContext.TelegramUsers.FindAsync(id);
            
            return dbUser;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }

    public async Task<TelegramUser?> FindByUsername(string name)
    {
        try
        {
            var dbUser = await dbContext.TelegramUsers.Where(tu => tu.UserName == name).FirstOrDefaultAsync();

            return dbUser;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }

    public async Task<bool> CheckIfExist(TelegramUser user)
    {
        try
        {
            var dbUser = await FindById(user.Id);
            if (dbUser == null)
            {
                return await Create(user);
            }

            if (dbUser.FirstName != user.FirstName 
                || dbUser.LastName != user.LastName 
                || dbUser.UserName != user.UserName 
                || dbUser.LanguageCode != user.LanguageCode)
            {
                return await Update(dbUser, user);
            }

            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }

    public async Task<bool> Update(TelegramUser dbUser, TelegramUser user)
    {
        try
        {
            dbUser.Update(user);

            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }

    public async Task<bool> Create(TelegramUser user)
    {
        try
        {
            dbContext.TelegramUsers.Add(user);
            
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
            var dbUser = await FindById(id);
            if (dbUser == null)
                return false;

            dbContext.TelegramUsers.Remove(dbUser);
            var res = await dbContext.SaveChangesAsync();
            return res > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }
}