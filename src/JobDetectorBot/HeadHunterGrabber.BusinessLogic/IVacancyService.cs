using HeadHunterGrabber.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadHunterGrabber.BusinessLogic
{
	public interface IVacancyService
	{
		Task<List<VacancyResponse>> GetAllAsync();

		Task<VacancyResponse> GetByIdAsync(Guid id);

		Task AddAsync(VacancyCreateRequest dto);

		Task UpdateAsync(Guid id, VacancyCreateRequest dto);

		Task DeleteAsync(Guid id);
	}
}
