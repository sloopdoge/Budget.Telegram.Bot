using Budget.Telegram.Bot.Entity.Entities;

namespace Budget.Telegram.Bot.Business.Interfaces;

public interface IUsersGroupService
{
    Task<UsersGroup?> FindById(long id);
    Task<bool> Update(UsersGroup dbGroup, UsersGroup group);
    Task<bool> Create(UsersGroup group);
    Task<bool> Delete(long id);
    Task<bool> AddUser(long groupId, TelegramUser user);
    Task<List<TelegramUser>> GetUsers(long id);
    Task<bool> AddBudget(long groupId, Entity.Entities.Budget budget);
    Task<List<Entity.Entities.Budget>> GetBudgets(long id);
    Task<List<UsersGroup>> GetUserGroups(long userId);
}