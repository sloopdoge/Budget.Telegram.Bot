using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Budget.Telegram.Bot.Entity.Entities;

public class TelegramUser
{
    [Key]
    public required long Id { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public string? LanguageCode { get; set; }
    public long ChatId { get; set; }

    [JsonIgnore]
    public ICollection<UsersGroup> Groups { get; set; } = [];

    public void Update(TelegramUser user)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        UserName = user.UserName;
        LanguageCode = user.LanguageCode;
        ChatId = user.ChatId;
    }
}