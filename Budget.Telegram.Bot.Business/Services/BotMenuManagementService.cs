using Budget.Telegram.Bot.Business.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Budget.Telegram.Bot.Business.Services;

public class BotMenuManagementService(ILogger<BotMenuManagementService> logger, ITelegramBotClient telegramBotClient) : IBotMenuManagementService
{
    
}