using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.Parser.Dto
{
	[Flags]
	public enum SearchKeyWords
	{
		Name = 1,
		Company = 2,
		Description = 4,
	}
}
