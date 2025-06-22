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

		//[HttpPost(Name = "InsertVacancy")]
		//public async Task InsertVacancy(VacancyCreateRequest vacancy)
		//{
		//	await _vacancyService.AddAsync(vacancy);
		//}

		//[HttpGet(Name = "GetVacancies")]
		//public async Task<List<VacancyResponse>> GetVacancies()
		//{
		//	return await _vacancyService.GetAllAsync();
		//}

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

	}
}
