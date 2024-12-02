using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Entity.Entities;
using Budget.Telegram.Bot.Entity.Enums.Menus;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Budget.Telegram.Bot.Business.Services;

public class BotMenuManagementService(ILogger<BotMenuManagementService> logger, ITelegramBotClient telegramBotClient, IBotMenuStateService botMenuStateService) : IBotMenuManagementService
{
    public async Task SetStartMenu(TelegramUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            var replyKeyboardMarkup = BuildMenu(MenuEnum.Start);
            botMenuStateService.PushMenu(user.Id, MenuEnum.Start);

            await telegramBotClient.SendMessage(
                chatId: user.ChatId,
                text: $"Welcome {user.FirstName} {user.LastName}. Select an option from menu.",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to set START menu.\n{e.Message}", e.StackTrace);
        }
    }

    public async Task SetGroupMenu(TelegramUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            var replyKeyboardMarkup = BuildMenu(MenuEnum.Groups);
            botMenuStateService.PushMenu(user.Id, MenuEnum.Start);
            
            await telegramBotClient.SendMessage(
                chatId: user.ChatId,
                text: $"Groups menu",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to set GROUP menu.\n{e.Message}", e.StackTrace);
        }
    }

    public async Task SetBudgetMenu(TelegramUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            var replyKeyboardMarkup = BuildMenu(MenuEnum.Budgets);
            botMenuStateService.PushMenu(user.Id, MenuEnum.Start);
            
            await telegramBotClient.SendMessage(
                chatId: user.ChatId,
                text: $"Budgets menu: Select the option.",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to set BUDGET menu.\n{e.Message}", e.StackTrace);
        }
    }

    public async Task HandleGoBack(TelegramUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            var previousMenu = botMenuStateService.PopMenu(user.Id);

            switch (previousMenu)
            {
                case MenuEnum.Start:
                    await SetStartMenu(user, cancellationToken);
                    break;
                case MenuEnum.Groups:
                    await SetStartMenu(user, cancellationToken);
                    break;
                case MenuEnum.Budgets:
                    await SetStartMenu(user, cancellationToken);
                    break;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to set PREVIOUS menu.\n{e.Message}", e.StackTrace);
        }
    }
    
    private ReplyKeyboardMarkup BuildMenu(MenuEnum menu)
    {
        var keyboard = menu switch
        {
            MenuEnum.Start => new[]
            {
                new KeyboardButton[] { StartMenuEnum.Groups.ToString(), StartMenuEnum.Budgets.ToString() }
            },
            MenuEnum.Groups => new[]
            {
                new KeyboardButton[] { "Group 1", "Group 2", MenuEnum.Back.ToString() }
            },
            MenuEnum.Budgets => new[]
            {
                new KeyboardButton[] { "Budget 1", "Budget 2", MenuEnum.Back.ToString() }
            },
            _ => new[]
            {
                new KeyboardButton[] { MenuEnum.Back.ToString() }
            }
        };

        return new ReplyKeyboardMarkup(keyboard)
        {
            ResizeKeyboard = true
        };
    }

}