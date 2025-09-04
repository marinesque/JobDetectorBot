using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.Dto
{
	public class VacancyRequest
	{
		public string Search { get; set; }
		
		public string? Experience { get; set; }
		
		public string? Employment { get; set; }
		
		public string? Schedule { get; set; }
		
		public string? SalaryRangeFrequency { get; set; }
		
		public int? Salary { get; set; }

		public string Region { get; set; }

		public int Page { get; set; }

		public bool? UseSimilarNames { get; set; }
	}
}
