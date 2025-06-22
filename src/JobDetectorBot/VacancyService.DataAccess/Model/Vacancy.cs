using MongoDB.Bson;
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
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

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
		/// Регион
		/// </summary>
		[BsonElement("area")]
		public Area? Area { get; set; }

		/// <summary>
		/// Тип вакансии
		/// </summary>
		[BsonElement("type")]
		public VacancyType? Type { get; set; }

		/// <summary>
		/// Адрес
		/// </summary>
		[BsonElement("address")]
		public Address? Address { get; set; }

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
		public Employment? Employment { get; set; }

		/// <summary>
		/// Занятость
		/// </summary>
		[BsonElement("employmentForm")]
		public EmploymentForm? EmploymentForm { get; set; }

		/// <summary>
		/// График
		/// </summary>
		[BsonElement("schedule")]
		public Schedule? Schedule { get; set; }

		/// <summary>
		/// Рабочие часы
		/// </summary>
		[BsonElement("workhours")]
		public List<WorkingHour> WorkingHours { get; set; }

		/// <summary>
		/// Формат работы
		/// </summary>
		[BsonElement("workschedule")]
		public List<WorkScheduleByDay> WorkScheduleByDays { get; set; }

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
		public List<WorkFormat> WorkFormat { get; set; }

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
		/// Дата публикации вакансии
		/// </summary>
		[BsonElement("published_at")]
		public DateTime? PublishedVacancyDate { get; set; }

		/// <summary>
		/// Компания
		/// </summary>
		[BsonElement("employer")]
		public Employer Employer { get; set; }

		/// <summary>
		/// Статус архивной вакансии
		/// </summary>
		[BsonElement("archived")]
		public bool? Archived { get; set; }

		[BsonElement("create")]
		public DateTime? Create { get; set; }

		[BsonElement("update")]
		public DateTime? Update { get; set; }

		[BsonElement("delete")]
		public DateTime? Delete {  get; set; }
	}
}
