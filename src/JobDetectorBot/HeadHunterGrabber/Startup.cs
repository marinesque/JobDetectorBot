using HeadHunterGrabber.BusinessLogic;
using HeadHunterGrabber.DataAccess.Model;
using HeadHunterGrabber.DataAccess.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace HeadHunterGrabber
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
		
			services.AddScoped(typeof(IRepository<Vacancy>), typeof(VacancyRepository));
			services.AddTransient(typeof(IVacancyService), typeof(VacancyService));

		}

		public void Configure(IApplicationBuilder app)
		{
			// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		}
	}
}
