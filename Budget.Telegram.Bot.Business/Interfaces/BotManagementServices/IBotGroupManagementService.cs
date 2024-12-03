using Budget.Telegram.Bot.Entity.Entities;

namespace Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;

public interface IBotGroupManagementService
{
    Task HandleAddGroup(TelegramUser user, string groupTitle = "", CancellationToken cancellationToken = default);
    Task HandleEditGroup(TelegramUser user, string message = "", CancellationToken cancellationToken = default);
    Task HandleInviteGroup(TelegramUser user, string userName = "", CancellationToken cancellationToken = default);
    Task HandleListGroups(TelegramUser user, string groupId = "", CancellationToken cancellationToken = default);
}