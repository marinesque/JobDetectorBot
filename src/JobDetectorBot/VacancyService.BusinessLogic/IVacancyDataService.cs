using VacancyService.Dto;

namespace VacancyService.BusinessLogic
{
	public interface IVacancyDataService
	{
		Task<List<VacancyResponse>> GetAllAsync();

		Task<VacancyResponse> GetByIdAsync(Guid id);

		Task AddAsync(VacancyCreateRequest dto);

		Task UpdateAsync(Guid id, VacancyCreateRequest dto);

		Task DeleteAsync(Guid id);

		Task<List<VacancyResponse>> FindVacancy(string search, bool name, bool company, bool description);
	}
}
