using Bot.Domain.DataAccess.Model;

namespace Bot.Domain.DataAccess.Dto
{
    public class CriteriaStepDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Prompt { get; set; }
        public bool IsCustom { get; set; }
        public int OrderBy { get; set; }
        public string Type { get; set; }
        public bool IsMapped { get; set; }
        public string? MainDictionary { get; set; }

        public CriteriaStep ToCriteriaStep()
        {
            return new CriteriaStep
            {
                Id = this.Id,
                Name = this.Name,
                Prompt = this.Prompt,
                IsCustom = this.IsCustom,
                OrderBy = this.OrderBy,
                Type = this.Type,
                IsMapped = this.IsMapped,
                MainDictionary = this.MainDictionary
            };
        }
    }
}
