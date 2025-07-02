namespace Bot.Domain.Request.VacancySearch
{
    public class CriteriaItem
    {
        public string CriteriaName { get; set; }
        public string Value { get; set; }
        public bool IsCustom { get; set; }
    }
}
