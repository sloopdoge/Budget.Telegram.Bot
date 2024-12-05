using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Budget.Telegram.Bot.Entity.Enums;

namespace Budget.Telegram.Bot.Entity.Entities;

public class Expense
{
    [Key]
    public long Id { get; set; }
    public ExpenseTypeEnum Type { get; set; }
    public required string Description { get; set; }
    public double Amount { get; set; }

    [JsonIgnore]
    public ICollection<Budget> Budgets { get; set; } = [];
    
    public void Update(Expense newExpense)
    {
        Description = newExpense.Description;
        Amount = newExpense.Amount;

        Budgets = newExpense.Budgets;
    }
}