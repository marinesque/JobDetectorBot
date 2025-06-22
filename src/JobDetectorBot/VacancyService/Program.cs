
using VacancyService.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace VacancyService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			//builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

			builder.Services.Configure<HeadHunterSettings>(builder.Configuration.GetSection("HeadHunterSettings"));

			//var mongoDBSettings = builder.Configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();

			var hhClientId = builder.Configuration["HeadHunter:ClientId"];

			var hhClientSecret = builder.Configuration["HeadHunter:ClientSecret"];

			//builder.Services.AddDbContext<VacancyDbContext>(options =>
			//	options.UseMongoDB(mongoDBSettings.ConnectionString ?? "", mongoDBSettings.DatabaseName ?? ""));

			var startup = new Startup(builder.Configuration);
			
			startup.ConfigureServices(builder.Services);

			var app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
