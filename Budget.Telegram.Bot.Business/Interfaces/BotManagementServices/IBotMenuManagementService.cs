using Budget.Telegram.Bot.Entity.Entities;

namespace Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;

public interface IBotMenuManagementService
{
    Task SetStartMenu(TelegramUser user, CancellationToken cancellationToken = default);
    Task SetGroupMenu(TelegramUser user, CancellationToken cancellationToken = default);
    Task SetBudgetMenu(TelegramUser user, CancellationToken cancellationToken = default);
    Task HandleGoBack(TelegramUser user, CancellationToken cancellationToken = default);
}