using Budget.Telegram.Bot.Entity.Enums.Menus;

namespace Budget.Telegram.Bot.Business.Interfaces;

public interface IBotSessionStateService
{
    void PushMenu(long userId, MenuEnum menu);
    MenuEnum? PopMenu(long userId);
    MenuEnum? GetCurrentMenu(long userId);
    void SetUserOperation(long userId, UserOperationsEnum operation);
    UserOperationsEnum? GetCurrentUserOperation(long userId);
    void ClearUserOperation(long userId);
}