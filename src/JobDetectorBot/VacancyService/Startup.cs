using VacancyService.BusinessLogic;
using VacancyService.Configuration;
using VacancyService.DataAccess.Model;
using VacancyService.DataAccess.Repository;
using VacancyService.Parser;
using VacancyService.Parser.Dto;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using VacancyService.HeadHunterApiClient;

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
			services.AddScoped(typeof(IRepository<Vacancy>), typeof(VacancyRepository));
			services.AddTransient(typeof(IVacancyDataService), typeof(VacancyDataService));

			services.AddTransient(typeof(IGrabberConfiguration), typeof(GrabberConfiguration));

			//services.AddTransient(typeof(IParser<SiteVacancy, SiteSearchParam>), typeof(SeleniumParser));
			services.AddTransient(typeof(IParser<SiteVacancy, SiteSearchParam>), typeof(HtmlAgilityParser));

			services.AddTransient(typeof(IServiceClient), typeof(ServiceClient));
		}

		public void Configure(IApplicationBuilder app)
		{
			// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		}
	}
}
