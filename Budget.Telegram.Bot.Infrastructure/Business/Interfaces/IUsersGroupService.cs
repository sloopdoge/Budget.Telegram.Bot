using Budget.Telegram.Bot.Domain.Entities;

namespace Budget.Telegram.Bot.Infrastructure.Business.Interfaces;

public interface IUsersGroupService
{
    Task<UsersGroup?> FindById(long id);
    Task<bool> Update(UsersGroup dbGroup, UsersGroup group);
    Task<bool> Create(UsersGroup group);
    Task<bool> Delete(long id);
    Task<bool> AddUser(long groupId, TelegramUser user);
    Task<List<TelegramUser>> GetUsers(long id);
    Task<bool> AddBudget(long groupId, Domain.Entities.Budget budget);
    Task<List<Domain.Entities.Budget>> GetBudgets(long id);
    Task<List<UsersGroup>> GetUserGroups(long userId);
}