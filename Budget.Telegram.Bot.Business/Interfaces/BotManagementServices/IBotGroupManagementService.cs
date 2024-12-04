using Budget.Telegram.Bot.Entity.Entities;
using Telegram.Bot.Types.Enums;

namespace Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;

public interface IBotGroupManagementService
{
    Task HandleAddGroup(TelegramUser user, string groupTitle = "", CancellationToken cancellationToken = default);
    Task HandleEditGroup(TelegramUser user, string message = "", CancellationToken cancellationToken = default);

    Task HandleInviteGroup(TelegramUser user, string message = "", UpdateType messageType = UpdateType.Message,
        CancellationToken cancellationToken = default);
    Task HandleListGroups(TelegramUser user, string groupId = "", CancellationToken cancellationToken = default);
}