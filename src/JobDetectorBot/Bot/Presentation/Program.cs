using Bot;
using Bot.Domain.DataAccess.Repositories;
using Bot.Infrastructure;
using Bot.Infrastructure.Interfaces;
using Bot.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using StackExchange.Redis;
using MessageHandler = Bot.Application.Handlers.MessageHandler;

internal class Program()
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            DisableDefaults = true
        });

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .AddCommandLine(args);

        // Проверка токена Telegram
        var telegramConfig = builder.Configuration.GetSection(BotOptions.Telegram);
        Console.WriteLine($"Telegram Token: {telegramConfig["Token"]}");
        if (string.IsNullOrEmpty(telegramConfig["Token"]))
        {
            throw new InvalidOperationException("Токен Telegram отсутствует в конфигурации.");
        }

        // Настройка базы данных
        var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
        Console.WriteLine($"connectionString: {connectionString}");
        builder.Services.AddDbContext<BotDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Redis
        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "Bot_";
        });
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        builder.Services.AddSingleton<IUserCacheService, UserCacheService>();
        builder.Services.AddHostedService<RedisSyncBackgroundService>();
        builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

        // Настройка сервисов
        Console.WriteLine("Настройка сервисов..");
        builder.Services.Configure<BotOptions>(telegramConfig);
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<BotOptions>>().Value);
        builder.Services.AddScoped<UserRepository>();
        builder.Services.AddScoped<CriteriaStepRepository>();
        builder.Services.AddTransient<DataSeeder>();
        builder.Services.AddScoped<IMessageHandler, MessageHandler>();
        builder.Services.AddScoped<ICriteriaStepsActualize, CriteriaStepsActualize>();
        builder.Services.AddHostedService<BotBackgroundService>();
        /*builder.Services.AddHttpClient<IVacancySearchService, VacancySearchService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["VacancySearchService:BaseUrl"]);
        });
        builder.Services.AddScoped<IVacancySearchService, VacancySearchService>();
        */

        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        Console.WriteLine("Билдим решение..");
        var host = builder.Build();
        Console.WriteLine("Билд завершен.");

        try
        {
            // Проверка парсинга строки подключения
            new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Invalid connection string format: {connectionString}", ex);
        }

        // Миграция?
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

            var retries = 10;
            while (retries > 0)
            {
                try
                {
                    dbContext.Database.Migrate();
                    break;
                }
                catch (Exception)
                {
                    retries--;
                    Thread.Sleep(5000);
                }
            }

            if (retries == 0)
            {
                throw new Exception("Не удалось подключиться к PostgreSQL.");
            }

            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            seeder.SeedDataAsync().GetAwaiter().GetResult();
        }

        try
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false");
            var db = redis.GetDatabase();

            db.StringSet("test_key", "test_value");
            var value = db.StringGet("test_key");

            Console.WriteLine($"Redis запущен. Value: {value}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis не удалось запустить: {ex.Message}");
        }

        host.Run();
    }
}
