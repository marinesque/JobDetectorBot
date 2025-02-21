using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadHunterGrabber.Parser
{
	public interface IParser<T, U> 
		where T : class
		where U : class
	{
		Task<List<T>> GetVacancies(U searchParam);

	}
}
