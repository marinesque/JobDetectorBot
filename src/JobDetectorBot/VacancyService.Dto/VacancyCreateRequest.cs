﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.Dto
{
	public class VacancyCreateRequest
	{
		/// <summary>
		/// Наименование вакансии
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Уровень дохода
		/// </summary>
		public string Salary { get; set; }

		/// <summary>
		/// Опыт работы
		/// </summary>
		public string WorkExperience { get; set; }

		/// <summary>
		/// Занятость
		/// </summary>
		public string Job { get; set; }

		/// <summary>
		/// График
		/// </summary>
		public string Schedule { get; set; }

		/// <summary>
		/// Рабочие часы
		/// </summary>
		public string WorkHours { get; set; }

		/// <summary>
		/// Формат работы
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Описание вакансии
		/// </summary>
		public string Description { get; set; }
	}
}
