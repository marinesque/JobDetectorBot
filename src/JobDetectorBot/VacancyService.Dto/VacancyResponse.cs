namespace VacancyService.Dto
{
	public class VacancyResponse
	{
		public string ExternalId { get; set; }

		/// <summary>
		/// Уникальное имя вакансии
		/// </summary>
		public string UniqName { get; set; }

		/// <summary>
		/// Наименование вакансии
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Регион
		/// </summary>
		public string Area { get; set; }

		/// <summary>
		/// Тип вакансии
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Адрес
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// Уровень дохода
		/// </summary>
		public SalaryRange? Salary { get; set; }

		/// <summary>
		/// Опыт работы
		/// </summary>
		public Experience WorkExperience { get; set; }

		/// <summary>
		/// Занятость
		/// </summary>
		public Employment? Employment { get; set; }

		/// <summary>
		/// Занятость
		/// </summary>
		public EmploymentForm? EmploymentForm { get; set; }

		/// <summary>
		/// График
		/// </summary>
		public string Schedule { get; set; }

		/// <summary>
		/// Рабочие часы
		/// </summary>
		public List<string> WorkingHours { get; set; }

		/// <summary>
		/// Формат работы
		/// </summary>
		public List<string> WorkScheduleByDays { get; set; }

		public List<string> ProfessionalRoles { get; set; }

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
		public List<string> WorkFormat { get; set; }

		/// <summary>
		/// Ссылка на вакансию
		/// </summary>
		public string Link { get; set; }

		/// <summary>
		/// Дата создания вакансии
		/// </summary>
		public DateTime? CreatedVacancyDate { get; set; }

		/// <summary>
		/// Дата публикации вакансии
		/// </summary>
		public DateTime? PublishedVacancyDate { get; set; }

		/// <summary>
		/// Компания
		/// </summary>
		public string Employer { get; set; }

		/// <summary>
		/// Статус архивной вакансии
		/// </summary>
		public bool? Archived { get; set; }

	}

}
