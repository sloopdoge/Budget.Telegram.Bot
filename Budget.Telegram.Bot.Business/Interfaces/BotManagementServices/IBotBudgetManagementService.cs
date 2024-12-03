using Budget.Telegram.Bot.Entity.Entities;

namespace Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;

public interface IBotBudgetManagementService
{
    Task HandleAddBudget(TelegramUser user, CancellationToken cancellationToken = default);
    Task HandleEditBudget(TelegramUser user, CancellationToken cancellationToken = default);
    Task HandleInviteBudget(TelegramUser user, CancellationToken cancellationToken = default);
}