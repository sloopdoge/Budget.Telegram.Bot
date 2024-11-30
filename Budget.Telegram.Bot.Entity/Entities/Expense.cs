﻿using System.Text.Json.Serialization;
using Budget.Telegram.Bot.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace Budget.Telegram.Bot.Entity.Entities;

public class Expense
{
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