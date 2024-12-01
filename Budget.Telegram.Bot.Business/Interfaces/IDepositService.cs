using Budget.Telegram.Bot.Entity.Entities;

namespace Budget.Telegram.Bot.Business.Interfaces;

public interface IDepositService
{
    Task<Deposit?> FindById(long id);
    Task<bool> Update(Deposit dbDeposit, Deposit deposit);
    Task<bool> Delete(long id);
}