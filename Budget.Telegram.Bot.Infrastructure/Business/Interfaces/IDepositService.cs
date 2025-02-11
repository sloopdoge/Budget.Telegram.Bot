using Budget.Telegram.Bot.Domain.Entities;

namespace Budget.Telegram.Bot.Infrastructure.Business.Interfaces;

public interface IDepositService
{
    Task<Deposit?> FindById(long id);
    Task<bool> Update(Deposit dbDeposit, Deposit deposit);
    Task<bool> Delete(long id);
}