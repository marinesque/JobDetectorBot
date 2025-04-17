namespace VacancyService.Dto
{
	public class VacancyResponse
	{
		/// <summary>
		/// Уникальное имя вакансии
		/// </summary>
		public string UniqName { get; set; }

		/// <summary>
		/// Наименование вакансии
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Уровень дохода
		/// </summary>
		public SalaryRange Salary { get; set; }

		/// <summary>
		/// Опыт работы
		/// </summary>
		public Experience WorkExperience { get; set; }

		/// <summary>
		/// Занятость
		/// </summary>
		public string Employment { get; set; }

		/// <summary>
		/// Занятость
		/// </summary>
		public string EmploymentForm { get; set; }

		/// <summary>
		/// График
		/// </summary>
		public string Schedule { get; set; }

		/// <summary>
		/// Рабочие часы
		/// </summary>
		public string[] WorkingHours { get; set; }

		/// <summary>
		/// Формат работы
		/// </summary>
		public string[] WorkScheduleByDays { get; set; }

		/// <summary>
		/// Требования
		/// </summary>
		public string Requirement { get; set; }

		/// <summary>
		/// Ответственность
		/// </summary>
		public string Responsibility { get; set; }

		/// <summary>
		/// Формат работы
		/// </summary>
		public string[] WorkFormat { get; set; }

		/// <summary>
		/// Ссылка на вакансию
		/// </summary>
		public string Link { get; set; }

		/// <summary>
		/// Дата создания вакансии
		/// </summary>
		public DateTime? CreatedVacancyDate { get; set; }

		/// <summary>
		/// Компания
		/// </summary>
		public string Company { get; set; }

	}
}
