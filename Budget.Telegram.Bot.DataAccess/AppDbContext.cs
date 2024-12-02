using Budget.Telegram.Bot.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget.Telegram.Bot.DataAccess;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TelegramUser> TelegramUsers { get; set; }
    public DbSet<UsersGroup> UsersGroups { get; set; }
    public DbSet<Entity.Entities.Budget> Budgets { get; set; }
    public DbSet<Deposit> Deposits { get; set; }
    public DbSet<Expense> Expenses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelegramUser>(entity =>
        {
            entity.HasKey(tu => tu.Id);
            entity.Property(tu => tu.Id).ValueGeneratedNever();
        });
        
        modelBuilder.Entity<TelegramUser>()
            .HasMany(tu => tu.Groups)
            .WithMany(g => g.Users)
            .UsingEntity<Dictionary<string, object>>(
                "TelegramUserUsersGroup",
                j => j.HasOne<UsersGroup>().WithMany().HasForeignKey("UsersGroupId"),
                j => j.HasOne<TelegramUser>().WithMany().HasForeignKey("TelegramUserId"));
        
        modelBuilder.Entity<UsersGroup>()
            .HasMany(us => us.Budgets)
            .WithMany(b => b.Groups)
            .UsingEntity<Dictionary<string, object>>(
                "UsersGroupBudget",
                j => j.HasOne<Entity.Entities.Budget>().WithMany().HasForeignKey("BudgetId"),
                j => j.HasOne<UsersGroup>().WithMany().HasForeignKey("UsersGroupId"));
        
        modelBuilder.Entity<Entity.Entities.Budget>()
            .HasMany(b => b.Deposits)
            .WithMany(d => d.Budgets)
            .UsingEntity<Dictionary<string, object>>(
                "BudgetDeposit",
                j => j.HasOne<Deposit>().WithMany().HasForeignKey("DepositId"),
                j => j.HasOne<Entity.Entities.Budget>().WithMany().HasForeignKey("BudgetId"));
        
        modelBuilder.Entity<Entity.Entities.Budget>()
            .HasMany(b => b.Expenses)
            .WithMany(e => e.Budgets)
            .UsingEntity<Dictionary<string, object>>(
                "BudgetExpense",
                j => j.HasOne<Expense>().WithMany().HasForeignKey("ExpenseId"),
                j => j.HasOne<Entity.Entities.Budget>().WithMany().HasForeignKey("BudgetId"));
    }
}