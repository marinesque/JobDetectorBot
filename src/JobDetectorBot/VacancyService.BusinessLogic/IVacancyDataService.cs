using VacancyService.Dto;

namespace VacancyService.BusinessLogic
{
	public interface IVacancyDataService
	{
		Task<List<VacancyResponse>> GetAllAsync();

		Task<VacancyResponse> GetByIdAsync(string id);

		Task DeleteAsync(string id);

		Task<List<VacancyResponse>> FindVacancy(string search, VacancySearchOptions searchOptions);

	}
}
