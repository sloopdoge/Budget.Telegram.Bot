using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Budget.Telegram.Bot.Domain.Enums;

namespace Budget.Telegram.Bot.Domain.Entities;

public class Expense
{
    [Key]
    public long Id { get; set; }
    public ExpenseTypeEnum Type { get; set; }
    public string Description { get; set; }
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