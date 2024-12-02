using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Entity.Entities;
using Budget.Telegram.Bot.Entity.Enums.Menus;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Budget.Telegram.Bot.Business.Services;

public class BotHandler(ILogger<BotHandler> logger, ITelegramUserService telegramUserService, IBotMenuManagementService botMenuManagementService)
{
    private TelegramUser? currentUser;
    
    public async Task HandleUpdate(Update update)
    {
        try
        {
            await InitCurrentUser(update);
            
            if (string.Equals($"/start", update.Message?.Text))
            {
                if (currentUser == null)
                    throw new Exception($"{nameof(HandleUpdate)} | User not found");

                await botMenuManagementService.SetStartMenu(currentUser);
                
                return;
            }

            switch (update.Message.Text)
            {
                case nameof(MenuEnum.Back):
                    await botMenuManagementService.HandleGoBack(currentUser);
                    return;
                case nameof(StartMenuEnum.Groups):
                    await botMenuManagementService.SetGroupMenu(currentUser);
                    return;
                case nameof(StartMenuEnum.Budgets):
                    await botMenuManagementService.SetBudgetMenu(currentUser);
                    return;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
        }
    }

    private async Task InitCurrentUser(Update update)
    {
        try
        {
            var user = update.Message?.From;
            if (user == null)
                throw new ArgumentNullException($"{nameof(HandleUpdate)} | User is not found");
                
            var userExist = await telegramUserService.CheckIfExist(new TelegramUser()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.Username,
                LanguageCode = user.LanguageCode,
                ChatId = update.Message.Chat.Id
            });
                
            if (!userExist)
                throw new Exception($"{nameof(HandleUpdate)} | User not found");
            
            currentUser = await telegramUserService.FindById(user.Id);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            throw;
        }
    }
}