using Budget.Telegram.Bot.Infrastructure.Business.Configs;
using Budget.Telegram.Bot.Infrastructure.Business.Interfaces;
using Budget.Telegram.Bot.Infrastructure.Business.Interfaces.BotManagementServices;
using Budget.Telegram.Bot.Infrastructure.Business.Interfaces.Helpers;
using Budget.Telegram.Bot.Infrastructure.Business.Services;
using Budget.Telegram.Bot.Infrastructure.Business.Services.BotManagementServices;
using Budget.Telegram.Bot.Infrastructure.Business.Services.Helpers;
using Budget.Telegram.Bot.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Telegram.Bot;

namespace Budget.Telegram.Bot.API;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        #region Serilog Logger
        
        var logsConnectionString = builder.Configuration.GetConnectionString("LogsConnection");

        if (builder.Environment.IsDevelopment())
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();
        }

        if (builder.Environment.IsProduction())
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.MSSqlServer(
                    connectionString: logsConnectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = true
                    },
                    restrictedToMinimumLevel: LogEventLevel.Warning)
                .CreateLogger();
        }
        
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog();
        
        #endregion
        
        Log.Warning("Starting web host");

        try
        {
            var token = builder.Configuration["TelegramBot:Token"];
            var webhookUrl = builder.Configuration["TelegramBot:WebhookUrl"];

            builder.Logging.ClearProviders();
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

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
            builder.Services.AddScoped<IBotHelper, BotHelper>();
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
        catch (Exception e)
        {
            Log.Fatal(e, e.Message);
        }
        finally
        {
            Log.Warning("Web host shutdown");
            await Log.CloseAndFlushAsync();
        }
    }
}