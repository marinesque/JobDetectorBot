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

		Task<Vacancy> GetByIdAsync(Guid id, CancellationToken token = default);

		Task AddAsync(Vacancy entity, CancellationToken token = default);

		Task BulkInsertAsync(List<Vacancy> entities, CancellationToken token = default);

		Task UpdateAsync(Guid id, Vacancy entity, CancellationToken token = default);

		Task DeleteAsync(Guid id, CancellationToken token = default);

		Task<List<Vacancy>> GetAllBySearchString(string search, DbSearchOptions dbSearchOptions, CancellationToken token = default);
	}
}
