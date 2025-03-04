namespace VacancyService.Dto
{
	public class VacancyResponse
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
	}
}
