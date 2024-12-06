using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Entity.Enums;
using Budget.Telegram.Bot.Entity.Enums.Menus;

namespace Budget.Telegram.Bot.Business.Services;

public class BotSessionStateService : IBotSessionStateService
{
    private readonly Dictionary<long, Stack<MenuEnum>> _menuHistory = new();
    private readonly Dictionary<long, UserOperationsEnum> _userOperations = new();
    private readonly Dictionary<long, long> _chosenGroups = new();
    private readonly Dictionary<long, Entity.Entities.Budget> _chosenBudgets = new();

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

    public MenuEnum GetCurrentMenuOrDefault(long userId) =>
        _menuHistory.TryGetValue(userId, out var stack) && stack.Any() ? stack.Peek() : MenuEnum.Start;
    
    public void SetUserOperation(long userId, UserOperationsEnum operation) =>
        _userOperations[userId] = operation;

    public UserOperationsEnum GetCurrentUserOperationOrDefault(long userId) =>
        _userOperations.TryGetValue(userId, out var operation) ? operation : UserOperationsEnum.None;

    public void ClearUserOperation(long userId) =>
        _userOperations.Remove(userId);
    
    public void PushUserChosenGroup(long userId, long groupId) => _chosenGroups.Add(userId, groupId);
    public long GetUserChosenGroup(long userId) => _chosenGroups[userId];
    public void RemoveUserChosenGroup(long userId) => _chosenGroups.Remove(userId);
    
    public void PushUserChosenBudget(long userId, Entity.Entities.Budget budget)
    {
        var isExist = _chosenBudgets.ContainsKey(userId);
        
        if (isExist)
            _chosenBudgets[userId] = budget;
        else
            _chosenBudgets.Add(userId, budget);
    }

    public Entity.Entities.Budget GetUserChosenBudget(long userId) => _chosenBudgets[userId];
    public void RemoveUserChosenBudget(long userId) => _chosenBudgets.Remove(userId);
}