namespace Bot.Domain.DataAccess.Dto
{
    public class VacancyDto
    {
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string Schedule { get; set; }
        public object Salary { get; set; }
        public string WorkFormat { get; set; }
        public string Url { get; set; }
        public DateTime PublishedDate { get; set; }
    }
}