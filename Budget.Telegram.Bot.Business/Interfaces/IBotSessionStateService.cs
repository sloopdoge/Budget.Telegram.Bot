using Budget.Telegram.Bot.Entity.Enums.Menus;

namespace Budget.Telegram.Bot.Business.Interfaces;

public interface IBotSessionStateService
{
    void PushMenu(long userId, MenuEnum menu);
    MenuEnum? PopMenu(long userId);
    MenuEnum GetCurrentMenuOrDefault(long userId);
    void SetUserOperation(long userId, UserOperationsEnum operation);
    UserOperationsEnum GetCurrentUserOperationOrDefault(long userId);
    void ClearUserOperation(long userId);
    void PushUserChosenGroup(long userId, long groupId);
    long GetUserChosenGroup(long userId);
    void RemoveUserChosenGroup(long userId);
}