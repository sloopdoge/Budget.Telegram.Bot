using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;
using Budget.Telegram.Bot.Entity.Entities;
using Budget.Telegram.Bot.Entity.Enums.Menus;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Budget.Telegram.Bot.Business.Services.BotManagementServices;

public class BotMenuManagementService(
    ILogger<BotMenuManagementService> logger,
    ITelegramBotClient telegramBotClient,
    IBotSessionStateService botSessionStateService)
    : IBotMenuManagementService
{
    public async Task SetStartMenu(TelegramUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            var replyKeyboardMarkup = BuildMainMenu(MenuEnum.Start);
            botSessionStateService.PushMenu(user.Id, MenuEnum.Start);

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
            var replyKeyboardMarkup = BuildMainMenu(MenuEnum.Groups);
            botSessionStateService.PushMenu(user.Id, MenuEnum.Groups);
            
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
            var replyKeyboardMarkup = BuildMainMenu(MenuEnum.Budgets);
            botSessionStateService.PushMenu(user.Id, MenuEnum.Budgets);
            
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
            var previousMenu = botSessionStateService.PopMenu(user.Id);
            botSessionStateService.ClearUserOperation(user.Id);

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
    
    private ReplyKeyboardMarkup BuildMainMenu(MenuEnum menu)
    {
        var keyboard = menu switch
        {
            MenuEnum.Start => new[]
            {
                new KeyboardButton[]
                {
                    StartMenuEnum.Groups.ToString(), 
                    StartMenuEnum.Budgets.ToString()
                }
            },
            MenuEnum.Groups => new[]
            {
                new KeyboardButton[]
                {
                    GroupMenuEnum.AddGroup.ToString(), 
                    GroupMenuEnum.EditGroup.ToString(),
                    GroupMenuEnum.InviteToGroup.ToString(), 
                    GroupMenuEnum.ListMyGroups.ToString(),
                    MenuEnum.Back.ToString()
                }
            },
            MenuEnum.Budgets => new[]
            {
                new KeyboardButton[] { BudgetMenuEnum.AddBudget.ToString(), 
                    BudgetMenuEnum.EditBudget.ToString(),
                    BudgetMenuEnum.ListMyBudgets.ToString(),
                    MenuEnum.Back.ToString(),
                }
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