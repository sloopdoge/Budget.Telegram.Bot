using Budget.Telegram.Bot.Domain.Entities;

namespace Budget.Telegram.Bot.Infrastructure.Business.Interfaces.BotManagementServices;

public interface IBotMenuManagementService
{
    Task SetStartMenu(TelegramUser user, CancellationToken cancellationToken = default);
    Task SetGroupMenu(TelegramUser user, CancellationToken cancellationToken = default);
    Task SetBudgetMenu(TelegramUser user, CancellationToken cancellationToken = default);
    Task HandleGoBack(TelegramUser user, CancellationToken cancellationToken = default);
}