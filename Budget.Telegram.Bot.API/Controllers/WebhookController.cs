using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Budget.Telegram.Bot.API.Controllers;

[ApiController]
[Route("api/bot/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<WebhookController> Logger;

    public WebhookController(ITelegramBotClient botClient, ILogger<WebhookController> logger)
    {
        _botClient = botClient;
        Logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        if (update == null)
        {
            Logger.LogError("Received an empty update");
            return BadRequest();
        }

        try
        {
            Logger.LogInformation($"Received update: {update.Id}");

            if (update.Message != null)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;

                Logger.LogInformation($"Message from {chatId}: {messageText}");

                await _botClient.SendMessage(chatId, $"You said: {messageText}");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error processing update: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}