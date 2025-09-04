using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.DataAccess.Model
{
	public class DbSearchOptions
	{
		/// <summary>
		/// Опыт работы
		/// </summary>
		public string Experience { get; set; }

		/// <summary>
		/// Занятость
		/// </summary>
		public string Employment { get; set; }

		/// <summary>
		/// График работы
		/// </summary>
		public string Schedule { get; set; }

		/// <summary>
		/// Частота выплат
		/// </summary>
		public string SalaryRangeFrequency { get; set; }

		/// <summary>
		/// Уровень дохода
		/// </summary>
		public int? Salary { get; set; }

		/// <summary>
		/// Страница
		/// </summary>
		public int? Page { get; set; }

		/// <summary>
		/// Регион
		/// </summary>
		public string Region { get; set; }

		public bool? UseSimilarNames { get; set; }

		public DateTime? DateTime { get; set; }
	}
}
