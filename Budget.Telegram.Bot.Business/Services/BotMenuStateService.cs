using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Entity.Enums.Menus;

namespace Budget.Telegram.Bot.Business.Services;

public class BotMenuStateService : IBotMenuStateService
{
    private readonly Dictionary<long, Stack<MenuEnum>> _menuHistory = new();

    public void PushMenu(long userId, MenuEnum menu)
    {
        if (!_menuHistory.ContainsKey(userId))
        {
            _menuHistory[userId] = new Stack<MenuEnum>();
        }
        _menuHistory[userId].Push(menu);
    }

    public MenuEnum? PopMenu(long userId)
    {
        if (_menuHistory.ContainsKey(userId) && _menuHistory[userId].Count > 0)
        {
            _menuHistory[userId].Pop();
            return _menuHistory[userId].Count > 0 ? _menuHistory[userId].Peek() : MenuEnum.Start;
        }
        return MenuEnum.Start;
    }

    public MenuEnum? GetCurrentMenu(long userId)
    {
        return _menuHistory.ContainsKey(userId) && _menuHistory[userId].Count > 0
            ? _menuHistory[userId].Peek()
            : MenuEnum.Start;
    }
}