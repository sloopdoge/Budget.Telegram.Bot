using Telegram.Bot;

namespace Budget.Telegram.Bot.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var token = builder.Configuration["TelegramBot:Token"];
            return new TelegramBotClient(token);
        });

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
        var webhookUrl = builder.Configuration["TelegramBot:WebhookUrl"];
        await botClient.SetWebhook(webhookUrl);

        await app.RunAsync();
    }
}