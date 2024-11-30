using Budget.Telegram.Bot.Entity.Entities;

namespace Budget.Telegram.Bot.Business.Interfaces;

public interface ITelegramUserService
{
    Task<TelegramUser?> FindById(long id);
    Task<bool> CheckIfExist(TelegramUser user);
    Task<bool> Update(TelegramUser dbUser, TelegramUser user);
    Task<bool> Create(TelegramUser user);
    Task<bool> Delete(long id);
}