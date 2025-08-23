using Bot.Domain.DataAccess.Dto;
using Bot.Domain.Request.VacancySearch;

namespace Bot.Infrastructure.Interfaces
{
    public interface IVacancySearchService
    {
        Task<bool> SearchAndCacheVacancies(long userId, UserCriteriaRequest request);
        Task<List<VacancyDto>> GetVacanciesPage(long userId, int page, int pageSize = 5);
        Task ClearCache(long userId);
    }
}