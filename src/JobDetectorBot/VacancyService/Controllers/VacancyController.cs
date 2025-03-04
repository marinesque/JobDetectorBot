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

		[HttpPost(Name = "InsertVacancy")]
		public async Task InsertVacancy(VacancyCreateRequest vacancy)
		{
			await _vacancyService.AddAsync(vacancy);
		}

		[HttpGet(Name = "GetVacancies")]
		public async Task<List<VacancyResponse>> GetVacancies()
		{
			return await _vacancyService.GetAllAsync();
		}

		[HttpGet("{search}", Name = "GetVacanciesFromSite")]
		public async Task<List<VacancyResponse>> GetVacanciesFromSite(string search, bool name, bool company, bool description)
		{
			return await _vacancyService.FindVacancy(search, name, company, description);
		}

	}
}
