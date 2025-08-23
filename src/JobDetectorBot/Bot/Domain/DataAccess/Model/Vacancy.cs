namespace Bot.Domain.DataAccess.Model
{
    public class Vacancy
    {
        public string ExternalId { get; set; }
        public string UniqName { get; set; }
        public string Name { get; set; }
        public string Area { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        public object Salary { get; set; }
        public object WorkExperience { get; set; }
        public object Employment { get; set; }
        public object EmploymentForm { get; set; }
        public string Schedule { get; set; }
        public List<string> WorkingHours { get; set; }
        public List<string> WorkScheduleByDays { get; set; }
        public string Requirement { get; set; }
        public string Responsibility { get; set; }
        public List<string> WorkFormat { get; set; }
        public string Link { get; set; }
        public DateTime CreatedVacancyDate { get; set; }
        public DateTime PublishedVacancyDate { get; set; }
        public string Employer { get; set; }
        public bool Archived { get; set; }
    }
}