using VacancyService.BusinessLogic;
using VacancyService.DataAccess.Model;
using VacancyService.DataAccess.Repository;
using VacancyService.Dto;
using Microsoft.AspNetCore.Mvc;

namespace VacancyService.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class VacancyController : ControllerBase
	{

		private readonly ILogger<VacancyController> _logger;

		private readonly IVacancyDataService _vacancyService;

		public VacancyController(ILogger<VacancyController> logger,
			IVacancyDataService vacancyService)
		{
			_logger = logger;
			_vacancyService = vacancyService;
		}


		[HttpGet("{search}", Name = "GetVacancies")]
		public async Task<List<VacancyResponse>> GetVacancies(
			string search, 
			string? experience, 
			string? employment,
			string? schedule,
			string? salaryRangeFrequency,
			int? salary
			)
		{
			var searchOptions = new VacancySearchOptions
			{
				Employment = employment,
				Experience = experience,
				Salary = salary,
				SalaryRangeFrequency = salaryRangeFrequency,
				Schedule = schedule
			};

			return await _vacancyService.FindVacancy(search, searchOptions);
		}

		[HttpPost(Name = "GetVacanciesByPage")]
		public async Task<List<VacancyResponse>> GetVacanciesByPage(
			VacancyRequest vacancyRequest
			)
		{
			var searchOptions = new VacancySearchOptions
			{
				Employment = vacancyRequest.Employment,
				Experience = vacancyRequest.Experience,
				Salary = vacancyRequest.Salary,
				SalaryRangeFrequency = vacancyRequest.SalaryRangeFrequency,
				Schedule = vacancyRequest.Schedule,
				Region = vacancyRequest.Region,
				Page = vacancyRequest.Page,
				UseSimilarNames = vacancyRequest.UseSimilarNames ?? true,
				DateTime = DateTime.UtcNow
			};

			return await _vacancyService.FindVacancy(vacancyRequest.Search, searchOptions);			
		}

	}
}
