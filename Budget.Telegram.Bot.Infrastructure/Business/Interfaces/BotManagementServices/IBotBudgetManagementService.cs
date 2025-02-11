using Budget.Telegram.Bot.Domain.Entities;

namespace Budget.Telegram.Bot.Infrastructure.Business.Interfaces.BotManagementServices;

public interface IBotBudgetManagementService
{
    Task HandleAddBudget(TelegramUser user, string message = "", CancellationToken cancellationToken = default);
    Task HandleEditBudget(TelegramUser user, string message = "", CancellationToken cancellationToken = default);
    Task HandleListBudget(TelegramUser user, CancellationToken cancellationToken = default);
    Task AddNewExpense(TelegramUser user, string message = "", CancellationToken cancellationToken = default);
    Task AddNewDeposit(TelegramUser user, string message = "", CancellationToken cancellationToken = default);
}