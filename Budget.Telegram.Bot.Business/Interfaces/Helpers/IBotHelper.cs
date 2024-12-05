using Telegram.Bot.Types.ReplyMarkups;

namespace Budget.Telegram.Bot.Business.Interfaces.Helpers;

public interface IBotHelper
{
    Task SendMessage(long chatId, string text, IReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default);

    InlineKeyboardMarkup BuildInlineKeyboard(IEnumerable<InlineKeyboardButton[]> buttons);
}