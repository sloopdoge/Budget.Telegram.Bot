using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Budget.Telegram.Bot.API.Controllers;

[ApiController]
[Route("api/bot/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ITelegramBotClient _botClient;

    public WebhookController(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        if (update.Message?.Text != null)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            await _botClient.SendMessage(chatId, $"You said: {messageText}");
        }

        return Ok();
    }
}