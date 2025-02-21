using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadHunterGrabber.Parser.Dto
{
	[Flags]
	public enum SearchKeyWords
	{
		Name = 0,
		Company = 1,
		Description = 2,
	}
}
