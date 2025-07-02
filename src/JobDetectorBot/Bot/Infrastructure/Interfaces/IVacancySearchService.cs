using Bot.Domain.DataAccess.Model;
using Bot.Domain.Request.VacancySearch;

namespace Bot.Infrastructure.Interfaces
{
    public interface IVacancySearchService
    {
        public Task SendUserCriteriaToSearchService(long userId);
        public Task<List<Vacancy>> SearchVacancies(UserCriteriaRequest request);
    }
}
