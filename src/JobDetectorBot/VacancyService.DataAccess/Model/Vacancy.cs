using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.DataAccess.Model
{
	public class Vacancy
	{
		[BsonId]
		public Guid Id { get; set; }

		[BsonElement("externalid")]
		public string ExternalId { get; set; }

		/// <summary>
		/// Уникальное имя вакансии
		/// </summary>
		[BsonElement("uniqname")]
		public string UniqName { get; set; }

		/// <summary>
		/// Наименование вакансии
		/// </summary>
		[BsonElement("name")]
		public string Name { get; set; }

		/// <summary>
		/// Уровень дохода
		/// </summary>
		[BsonElement("salary")]
		public SalaryRange? Salary { get; set; }

		/// <summary>
		/// Опыт работы
		/// </summary>
		[BsonElement("workexperience")]
		public Experience WorkExperience { get; set; }

		/// <summary>
		/// Занятость
		/// </summary>
		[BsonElement("employment")]
		public string Employment { get; set; }

		/// <summary>
		/// Занятость
		/// </summary>
		[BsonElement("employmentForm")]
		public string EmploymentForm { get; set; }

		/// <summary>
		/// График
		/// </summary>
		[BsonElement("schedule")]
		public string Schedule { get; set; }

		/// <summary>
		/// Рабочие часы
		/// </summary>
		[BsonElement("workhours")]
		public string[] WorkingHours { get; set; }

		/// <summary>
		/// Формат работы
		/// </summary>
		[BsonElement("workschedule")]
		public string[] WorkScheduleByDays { get; set; }

		/// <summary>
		/// Требования
		/// </summary>
		[BsonElement("requirement")]
		public string Requirement { get; set; }

		/// <summary>
		/// Ответственность
		/// </summary>
		[BsonElement("responsibility")]
		public string Responsibility { get; set; }

		/// <summary>
		/// Формат работы
		/// </summary>
		[BsonElement("workformat")]
		public string[] WorkFormat { get; set; }

		/// <summary>
		/// Ссылка на вакансию
		/// </summary>
		[BsonElement("link")]
		public string Link { get; set; }

		/// <summary>
		/// Дата создания вакансии
		/// </summary>
		[BsonElement("created_at")]
		public DateTime? CreatedVacancyDate { get; set; }

		/// <summary>
		/// Компания
		/// </summary>
		[BsonElement("company")]
		public string Company { get; set; }

		[BsonElement("create")]
		public DateTime? Create { get; set; }

		[BsonElement("update")]
		public DateTime? Update { get; set; }

		[BsonElement("delete")]
		public DateTime? Delete {  get; set; }

	}
}
