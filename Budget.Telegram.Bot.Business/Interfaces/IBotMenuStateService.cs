using Budget.Telegram.Bot.Entity.Enums.Menus;

namespace Budget.Telegram.Bot.Business.Interfaces;

public interface IBotMenuStateService
{
    void PushMenu(long userId, MenuEnum menu);
    MenuEnum? PopMenu(long userId);
    MenuEnum? GetCurrentMenu(long userId);
}