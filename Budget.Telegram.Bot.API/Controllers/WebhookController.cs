using Budget.Telegram.Bot.Infrastructure.Business.Services;
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
    private readonly IConfiguration _configuration;
    private readonly BotHandler _botHandler;

    private string token = "";
    private string webhookUrl = "";

    public WebhookController(ITelegramBotClient botClient, ILogger<WebhookController> logger, IConfiguration configuration, BotHandler botHandler)
    {
        _botClient = botClient;
        Logger = logger;
        _configuration = configuration;
        _botHandler = botHandler;

        var token = _configuration["TelegramBot:Token"];
        var webhookUrl = _configuration["TelegramBot:WebhookUrl"];
    }
    
    [HttpPost]
    public async Task<IActionResult> HandleUpdate([FromBody] Update update)
    {
        try
        {
            if (update == null)
            {
                throw new NullReferenceException("Received empty update.");
            }

            await _botHandler.HandleUpdate(update);

            return Ok();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            return StatusCode(500);
        }
    }
    
    [HttpGet]
    public string Get() 
    {
        return "Telegram bot was started";
    }
}