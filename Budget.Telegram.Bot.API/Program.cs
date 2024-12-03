using Budget.Telegram.Bot.Business.Configs;
using Budget.Telegram.Bot.Business.Interfaces;
using Budget.Telegram.Bot.Business.Interfaces.BotManagementServices;
using Budget.Telegram.Bot.Business.Services;
using Budget.Telegram.Bot.Business.Services.BotManagementServices;
using Budget.Telegram.Bot.DataAccess;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace Budget.Telegram.Bot.API;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var token = builder.Configuration["TelegramBot:Token"];
        var webhookUrl = builder.Configuration["TelegramBot:WebhookUrl"];

        builder.Services.AddControllers();
        builder.Services.ConfigureTelegramBot<Microsoft.AspNetCore.Mvc.JsonOptions>(options => options.JsonSerializerOptions);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddSingleton<ITelegramBotClient>(options => new TelegramBotClient(token));
        builder.Services.AddSingleton<IBotSessionStateService, BotSessionStateService>();
        builder.Services.AddSingleton<BotMenusConfig>();

        builder.Services.AddScoped<ITelegramUserService, TelegramUserService>();
        builder.Services.AddScoped<IUsersGroupService, UsersGroupService>();
        builder.Services.AddScoped<IBudgetService, BudgetService>();
        builder.Services.AddScoped<IDepositService, DepositService>();
        builder.Services.AddScoped<IExpenseService, ExpenseService>();
        builder.Services.AddScoped<IBotMenuManagementService, BotMenuManagementService>();
        builder.Services.AddScoped<IBotGroupManagementService, BotGroupManagementService>();
        builder.Services.AddScoped<IBotBudgetManagementService, BotBudgetManagementService>();
        builder.Services.AddScoped<BotHandler>();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        
        var botClient = app.Services.GetRequiredService<ITelegramBotClient>();
        await botClient.SetWebhook($"{webhookUrl}/api/bot/Webhook");

        await app.RunAsync();
    }
}