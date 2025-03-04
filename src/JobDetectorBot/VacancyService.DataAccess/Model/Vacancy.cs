using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.DataAccess.Model
{
	public class Vacancy
	{
		public Guid Id { get; set; }

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
		public string WorkTime { get; set; }

		/// <summary>
		/// Формат работы
		/// </summary>
		public string WorkType { get; set; }

		/// <summary>
		/// Описание вакансии
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Формат работы
		/// </summary>
		public string WorkFormat { get; set; }

		/// <summary>
		/// Ссылка на вакансию
		/// </summary>
		public string Link { get; set; }

		/// <summary>
		/// Дата создания вакансии
		/// </summary>
		public string CreatedVacancyDate { get; set; }

		/// <summary>
		/// Компания
		/// </summary>
		public string Company { get; set; }

		public DateTime? Create { get; set; }

		public DateTime? Update { get; set; }

		public DateTime? Delete {  get; set; }
	}
}
