﻿using Bot;
using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using Bot.Infrastructure;
using Bot.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MessageHandler = Bot.Application.Handlers.MessageHandler;

internal class Program()
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Проверка токена Telegram
        var telegramConfig = builder.Configuration.GetSection(BotOptions.Telegram);
        Console.WriteLine($"Telegram Token: {telegramConfig["Token"]}");
        if (string.IsNullOrEmpty(telegramConfig["Token"]))
        {
            throw new InvalidOperationException("Токен Telegram отсутствует в конфигурации.");
        }

        // Настройка базы данных
        var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
        builder.Services.AddDbContext<BotDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Настройка сервисов
        builder.Services.Configure<BotOptions>(telegramConfig);
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<BotOptions>>().Value);
        builder.Services.AddScoped<UserRepository>();
        builder.Services.AddScoped<CriteriaStepRepository>();
        builder.Services.AddTransient<DataSeeder>();
        builder.Services.AddScoped<IMessageHandler, MessageHandler>();
        builder.Services.AddScoped<ICriteriaStepsActualize, CriteriaStepsActualize>();
        builder.Services.AddHostedService<BotBackgroundService>();

        var host = builder.Build();

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

        host.Run();
    }
}
