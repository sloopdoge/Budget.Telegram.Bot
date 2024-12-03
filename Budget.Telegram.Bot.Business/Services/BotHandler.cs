using Budget.Telegram.Bot.Business.Configs;
using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;
using Budget.Telegram.Bot.Entity.Entities;
using Budget.Telegram.Bot.Entity.Enums.Menus;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Budget.Telegram.Bot.Business.Services;

public class BotHandler(
    ILogger<BotHandler> logger, 
    ITelegramBotClient botClient,
    ITelegramUserService telegramUserService, 
    IBotMenuManagementService botMenuManagementService, 
    IBotSessionStateService botSessionStateService,
    IBotGroupManagementService botGroupManagementService,
    IBotBudgetManagementService botBudgetManagementService,
    BotMenusConfig botMenusConfig)
{
    private TelegramUser? _currentUser;
    private MenuEnum? _currentMainMenu;
    private UserOperationsEnum? _currentUserOperation;
    
    public async Task HandleUpdate(Update update)
    {
        try
        {
            if (update == null)
                throw new ArgumentNullException(nameof(update));
            
            await InitCurrentUser(update);
            InitCurrentMenu(update);
            InitCurrentOperation(update);
            
            if (_currentUser == null)
                throw new Exception($"{nameof(HandleUpdate)} | User not found");
            
            if (string.Equals($"/start", update.Message?.Text))
            {
                await botMenuManagementService.SetStartMenu(_currentUser);
                
                return;
            }

            switch (_currentMainMenu)
            {
                case MenuEnum.Groups:
                    switch (_currentUserOperation)
                    {
                        case UserOperationsEnum.AddGroup:
                            if (string.Equals(update.Message.Text, "Cancel"))
                            {
                                await botMenuManagementService.SetGroupMenu(_currentUser);
                                return;
                            }
                        
                            await botGroupManagementService.HandleAddGroup(_currentUser, update.Message.Text);
                            break;
                        
                        case UserOperationsEnum.EditGroup:
                            if (string.Equals(update.Message.Text, "Cancel"))
                            {
                                await botMenuManagementService.SetGroupMenu(_currentUser);
                                return;
                            }

                            if (update.Type == UpdateType.CallbackQuery)
                            {
                                var callbackData = update.CallbackQuery.Data;
                                
                                await botGroupManagementService.HandleEditGroup(_currentUser, callbackData);
                            }
                            
                            await botGroupManagementService.HandleEditGroup(_currentUser, update.Message.Text);
                            
                            return;
                    }
                    break;
            }

            switch (update.Message?.Text)
            {
                case nameof(MenuEnum.Back):
                    await botMenuManagementService.HandleGoBack(_currentUser);
                    return;
                case nameof(StartMenuEnum.Groups):
                    await botMenuManagementService.SetGroupMenu(_currentUser);
                    return;
                case nameof(StartMenuEnum.Budgets):
                    await botMenuManagementService.SetBudgetMenu(_currentUser);
                    return;
                case nameof(GroupMenuEnum.AddGroup):
                    await botGroupManagementService.HandleAddGroup(_currentUser);
                    return;
                case nameof(GroupMenuEnum.EditGroup):
                    await botGroupManagementService.HandleEditGroup(_currentUser);
                    return;
                case nameof(GroupMenuEnum.InviteToGroup):
                    await botGroupManagementService.HandleInviteGroup(_currentUser);
                    return;
                case nameof(GroupMenuEnum.ListMyGroups):
                    
                    return;
                case nameof(BudgetMenuEnum.AddBudget):
                    
                    return;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            await botClient.SendMessage(_currentUser.ChatId, $"Sorry something went wrong");
        }
    }

    private void InitCurrentOperation(Update update)
    {
        _currentUserOperation = botSessionStateService.GetCurrentUserOperation(_currentUser.Id);
    }

    private void InitCurrentMenu(Update update)
    {
        _currentMainMenu = botSessionStateService.GetCurrentMenu(_currentUser.Id);
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
            
            _currentUser = await telegramUserService.FindById(user.Id);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message, e.StackTrace);
            throw;
        }
    }
}