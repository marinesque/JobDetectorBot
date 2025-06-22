using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacancyService.HeadHunterApiClient.Dto;
using VacancyService.HeadHunterApiClient.Dto.VacancyPositions;

namespace VacancyService.HeadHunterApiClient
{
	public interface IServiceClient
	{
		Task<Root> GetVacancies(string searchText, Dictionary<string, string> searchParams);

		Task<VacancyPosition> GetVacancyPositions(string searchText);

	}
}
