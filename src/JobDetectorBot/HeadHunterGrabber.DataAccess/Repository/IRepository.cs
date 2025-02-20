using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadHunterGrabber.DataAccess.Repository
{
	public interface IRepository<T> where T : class
	{
		Task<List<T>> GetAllAsync();

		Task<T> GetByIdAsync(Guid id);

		Task AddAsync(T entity);

		Task UpdateAsync(Guid id, T entity);

		Task DeleteAsync(Guid id);
	}
}
