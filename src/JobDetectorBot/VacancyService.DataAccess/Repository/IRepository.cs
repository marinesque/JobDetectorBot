using VacancyService.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.DataAccess.Repository
{
	public interface IRepository<T> where T : class
	{
		Task<List<T>> GetAllAsync();

		Task<T> GetByIdAsync(Guid id);

		Task AddAsync(T entity);

		Task BulkInsertAsync(List<T> entities);

		Task UpdateAsync(Guid id, T entity);

		Task DeleteAsync(Guid id);

		Task<List<T>> GetAllBySearchString(string search, DbSearchOptions dbSearchOptions);
	}
}
