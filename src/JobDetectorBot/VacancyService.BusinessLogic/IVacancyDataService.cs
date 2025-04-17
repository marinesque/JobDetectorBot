using VacancyService.Dto;

namespace VacancyService.BusinessLogic
{
	public interface IVacancyDataService
	{
		Task<List<VacancyResponse>> GetAllAsync();

		Task<VacancyResponse> GetByIdAsync(Guid id);

		Task DeleteAsync(Guid id);

		Task<List<VacancyResponse>> FindVacancy(string search, VacancySearchOptions searchOptions);
	}
}
