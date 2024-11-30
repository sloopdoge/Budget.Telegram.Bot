using Microsoft.Extensions.Logging;
using Telegram.Bots.Types;

namespace Budget.Telegram.Bot.Business.Services;

public class BotHandler(ILogger<BotHandler> logger)
{
    private readonly ILogger<BotHandler> Logger = logger;

    public async Task HandleUpdate(Update update)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message, e.StackTrace);
        }
    }
}