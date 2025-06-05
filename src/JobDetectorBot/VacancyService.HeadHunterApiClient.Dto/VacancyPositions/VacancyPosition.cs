using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.HeadHunterApiClient.Dto.VacancyPositions
{
	public class VacancyPositionItem
	{
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		[JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
		public string Text { get; set; }

		[JsonProperty("professional_roles", NullValueHandling = NullValueHandling.Ignore)]
		public List<VacancyPositionProfessionalRole> ProfessionalRoles { get; set; }
	}

	public class VacancyPositionProfessionalRole
	{
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		public string Name { get; set; }

		[JsonProperty("accept_incomplete_resumes", NullValueHandling = NullValueHandling.Ignore)]
		public bool AcceptIncompleteResumes { get; set; }
	}

	public class VacancyPosition
	{
		[JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
		public List<VacancyPositionItem> Items { get; set; }
	}
}
