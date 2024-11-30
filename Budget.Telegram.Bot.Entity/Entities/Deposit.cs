using System.Text.Json.Serialization;

namespace Budget.Telegram.Bot.Entity.Entities;

public class Deposit
{
    public long Id { get; set; }
    public string? Description { get; set; }
    public double Amount { get; set; }

    [JsonIgnore]
    public ICollection<Budget> Budgets { get; set; } = [];

    public void Update(Deposit newDeposit)
    {
        Description = newDeposit.Description;
        Amount = newDeposit.Amount;

        Budgets = newDeposit.Budgets;
    }
}