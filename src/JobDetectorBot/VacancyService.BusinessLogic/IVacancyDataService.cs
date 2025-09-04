using VacancyService.Dto;

namespace VacancyService.BusinessLogic
{
	public interface IVacancyDataService
	{
		/// <summary>
		/// Получить все вакансии
		/// </summary>
		/// <returns></returns>
		Task<List<VacancyResponse>> GetAllAsync();

		/// <summary>
		/// Получить вакансию по идентификатору
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<VacancyResponse> GetByIdAsync(string id);

		/// <summary>
		/// Удалить вакансию по идентификатору
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task DeleteAsync(string id);

		/// <summary>
		/// Найти вакансии по строке поиска
		/// </summary>
		/// <param name="search"></param>
		/// <param name="searchOptions"></param>
		/// <returns></returns>
		Task<List<VacancyResponse>> FindVacancy(string search, VacancySearchOptions searchOptions);

	}
}
