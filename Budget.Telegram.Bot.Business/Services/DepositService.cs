using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.DataAccess;
using Budget.Telegram.Bot.Entity.Entities;
using Microsoft.Extensions.Logging;

namespace Budget.Telegram.Bot.Business.Services;

public class DepositService(ILogger<DepositService> logger, AppDbContext dbContext) : IDepositService
{
    public async Task<Deposit?> FindById(long id)
    {
        try
        {
            var dbDeposit = await dbContext.Deposits.FindAsync(id);
            return dbDeposit;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            return null;
        }
    }

    public async Task<bool> Update(Deposit dbDeposit, Deposit deposit)
    {
        try
        {
            dbDeposit.Update(deposit);
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
            var dbDeposit = await FindById(id);
            if (dbDeposit == null)
                return false;

            dbContext.Deposits.Remove(dbDeposit);
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