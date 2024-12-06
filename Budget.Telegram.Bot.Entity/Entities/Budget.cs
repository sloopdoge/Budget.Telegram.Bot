using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Budget.Telegram.Bot.Entity.Entities;

public class Budget
{
    [Key]
    public long Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public double? Amount { get; set; } = 0;

    [JsonIgnore]
    public ICollection<UsersGroup> Groups { get; set; } = [];
    [JsonIgnore]
    public ICollection<Deposit> Deposits { get; set; } = [];
    [JsonIgnore]
    public ICollection<Expense> Expenses { get; set; } = [];

    public void Update(Budget newBudget)
    {
        Title = newBudget.Title;
        Description = newBudget.Description;

        Groups = newBudget.Groups;
        Deposits = newBudget.Deposits;
        Expenses = newBudget.Expenses;
    }
}