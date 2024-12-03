using Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;
using Budget.Telegram.Bot.Entity.Entities;
using Microsoft.Extensions.Logging;

namespace Budget.Telegram.Bot.Business.Services.BotManagementServices;

public class BotBudgetManagementService(ILogger<BotBudgetManagementService> logger) : IBotBudgetManagementService
{
    public async Task HandleAddBudget(TelegramUser user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task HandleEditBudget(TelegramUser user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task HandleInviteBudget(TelegramUser user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}