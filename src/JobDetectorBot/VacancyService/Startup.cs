using VacancyService.BusinessLogic;
using VacancyService.Configuration;
using VacancyService.DataAccess.Model;
using VacancyService.DataAccess.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using VacancyService.HeadHunterApiClient;
using StackExchange.Redis;

namespace VacancyService
{
	public class Startup
	{
		public IConfiguration Configuration { get; }


		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddHttpClient();
			services.AddScoped(typeof(IVacancyRepository), typeof(VacancyRepository));
			services.AddTransient(typeof(IVacancyDataService), typeof(VacancyDataService));

			services.AddTransient(typeof(IGrabberConfiguration), typeof(GrabberConfiguration));

			services.Configure<MongoDBSettings>(Configuration.GetSection("MongoDBSettings"));

			services.AddTransient(typeof(IServiceClient), typeof(ServiceClient));

			var redisConfig = Configuration.GetSection("Redis")["ConnectionString"];
			services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));
			services.AddTransient<IRedisQueryStore, RedisQueryStore>();

			var baseUrl = Configuration["HeadHunter:BaseUrl"];
			var apiKey = Configuration["HeadHunter:ApiKey"];
			services.AddSingleton<IServiceClientFactory>(provider =>
				new HeadHunterServiceClientFactory(
					baseUrl,
					apiKey,
					provider.GetRequiredService<IHttpClientFactory>()
				)
			);
			services.AddScoped<IServiceClient>(provider =>
				provider.GetRequiredService<IServiceClientFactory>().CreateClient());
		}

		public void Configure(IApplicationBuilder app)
		{
			// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		}
	}
}
