using Bot;
using Bot.Domain.DataAccess.Repositories;
using Bot.Infrastructure;
using Bot.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
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

            var retries = 5;
            while (retries > 0)
            {
                try
                {
                    Console.WriteLine("Попытка подключения к PostgreSQL...");

                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT version();";
                            var version = (string)command.ExecuteScalar();
                            Console.WriteLine($"Версия PostgreSQL: {version}");
                        }

                        connection.Close();
                    }

                    Console.WriteLine("Применение миграций...");
                    dbContext.Database.Migrate();
                    Console.WriteLine("Миграции успешно применены");
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

        host.Run();
    }
}
