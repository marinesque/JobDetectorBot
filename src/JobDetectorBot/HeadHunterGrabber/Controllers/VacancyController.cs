using HeadHunterGrabber.BusinessLogic;
using HeadHunterGrabber.DataAccess.Model;
using HeadHunterGrabber.DataAccess.Repository;
using HeadHunterGrabber.Dto;
using Microsoft.AspNetCore.Mvc;

namespace HeadHunterGrabber.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class VacancyController : ControllerBase
	{

		private readonly ILogger<VacancyController> _logger;

		private readonly IVacancyService _vacancyService;

		public VacancyController(ILogger<VacancyController> logger,
			IVacancyService vacancyService)
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

	}
}
