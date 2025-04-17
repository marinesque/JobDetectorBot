using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.Parser.Dto
{
	public class SiteSearchParam
	{
		public string SearchString { get; set; }

		public SearchKeyWords SearchKeyWords { get; set; }
	}
}
