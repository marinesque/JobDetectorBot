
using HeadHunterGrabber.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace HeadHunterGrabber
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

			builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

			builder.Services.Configure<HeadHunterSettings>(builder.Configuration.GetSection("HeadHunterSettings"));

			var mongoDBSettings = builder.Configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();

			builder.Services.AddDbContext<VacancyDbContext>(options =>
				options.UseMongoDB(mongoDBSettings.ConnectionString ?? "", mongoDBSettings.DatabaseName ?? ""));

			var startup = new Startup(builder.Configuration);

			startup.ConfigureServices(builder.Services);

			var app = builder.Build();

			//startup.Configure(app);

			// Configure the HTTP request pipeline.
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
