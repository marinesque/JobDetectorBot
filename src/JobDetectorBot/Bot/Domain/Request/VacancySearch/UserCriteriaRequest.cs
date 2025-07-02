namespace Bot.Domain.Request.VacancySearch
{
    public class UserCriteriaRequest
    {
        public long UserId { get; set; }
        public DateTime RequestDate { get; set; }
        public List<CriteriaItem> UserCriteria { get; set; } = new();
    }
}
