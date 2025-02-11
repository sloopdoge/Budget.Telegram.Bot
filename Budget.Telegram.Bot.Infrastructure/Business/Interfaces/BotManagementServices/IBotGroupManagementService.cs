using Budget.Telegram.Bot.Domain.Entities;

namespace Budget.Telegram.Bot.Infrastructure.Business.Interfaces.BotManagementServices;

public interface IBotGroupManagementService
{
    Task HandleAddGroup(TelegramUser user, string groupTitle = "", CancellationToken cancellationToken = default);
    Task HandleEditGroup(TelegramUser user, string message = "", CancellationToken cancellationToken = default);
    Task HandleInviteGroup(TelegramUser user, string message = "", CancellationToken cancellationToken = default);
    Task HandleListGroups(TelegramUser user, CancellationToken cancellationToken = default);
}