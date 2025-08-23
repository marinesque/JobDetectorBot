namespace Bot.Domain.Request.VacancySearch
{
    public class UserCriteriaItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool IsCustom { get; set; }
        public bool IsMapped { get; set; }
        public string? MainDictionary { get; set; }
    }
}
