using Budget.Telegram.Bot.Infrastructure.Business.Interfaces.Helpers;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Budget.Telegram.Bot.Infrastructure.Business.Services.Helpers;

public class BotHelper(ITelegramBotClient botClient, ILogger<BotHelper> logger) : IBotHelper
{
    public async Task SendMessage(long chatId, string text, IReplyMarkup? replyMarkup = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await botClient.SendMessage(chatId, text, replyMarkup: replyMarkup, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to send message to chat {chatId}. Error: {ex.Message}");
            throw;
        }
    }

    public InlineKeyboardMarkup BuildInlineKeyboard(IEnumerable<InlineKeyboardButton[]> buttons) =>
        new(buttons);
}