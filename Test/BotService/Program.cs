using BotService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

internal class Program()
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        // Проверка загруженной конфигурации
        var telegramConfig = builder.Configuration.GetSection(BotOptions.Telegram);
        Console.WriteLine($"Telegram Token: {telegramConfig["Token"]}");

        var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.Configure<BotOptions>(telegramConfig);
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<BotOptions>>().Value);
        builder.Services.AddSingleton<IMessageHandler, MessageHandler>();
        builder.Services.AddHostedService<BotBackgroundService>();

        var host = builder.Build();
        host.Run();
        Console.ReadLine();

    }
}
