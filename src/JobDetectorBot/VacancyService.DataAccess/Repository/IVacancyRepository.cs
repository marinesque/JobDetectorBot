using VacancyService.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.DataAccess.Repository
{
	public interface IVacancyRepository
	{
		Task<List<Vacancy>> GetAllAsync(CancellationToken token = default);

		Task<Vacancy> GetByIdAsync(string id, CancellationToken token = default);

		Task AddAsync(Vacancy entity, CancellationToken token = default);

		Task BulkInsertAsync(List<Vacancy> entities, CancellationToken token = default);

		Task UpdateAsync(string id, Vacancy entity, CancellationToken token = default);

		Task DeleteAsync(string id, CancellationToken token = default);

		Task<List<Vacancy>> GetAllBySearchString(string search, DbSearchOptions dbSearchOptions, CancellationToken token = default);

		Task<List<Vacancy>> GetAllBySearchString(Dictionary<string, List<string>> vacancyNames, DbSearchOptions dbSearchOptions, CancellationToken token = default);

		Task<List<string>> GetAllByExternalIdsAsync(List<string> externalIds, CancellationToken token = default);
	}
}
