﻿using System.Text.Json.Serialization;

namespace Budget.Telegram.Bot.Entity.Entities;

public class Budget
{
    public long Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }

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