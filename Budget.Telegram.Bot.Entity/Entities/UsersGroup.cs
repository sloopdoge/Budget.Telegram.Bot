using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Budget.Telegram.Bot.Entity.Entities;

public class UsersGroup
{
    [Key]
    public long Id { get; set; }
    public required string Title { get; set; }

    [JsonIgnore]
    public ICollection<TelegramUser> Users { get; set; } = [];
    [JsonIgnore]
    public ICollection<Budget> Budgets { get; set; } = [];
    
    public void Update(UsersGroup newGroup)
    {
        Title = newGroup.Title;

        Users = newGroup.Users;
        Budgets = newGroup.Budgets;
    }
}