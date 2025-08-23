using Bot.Domain.DataAccess.Model;

namespace Bot.Domain.DataAccess.Dto
{
    public class UserCriteriaStepValueDto
    {
        public long CriteriaStepId { get; set; }
        public long? CriteriaStepValueId { get; set; }
        public string? CustomValue { get; set; }

        // Добавляем поля для хранения данных CriteriaStepValue
        public string? CriteriaStepValuePrompt { get; set; }
        public string? CriteriaStepValueValue { get; set; }
        public int? CriteriaStepValueOrderBy { get; set; }

        public UserCriteriaStepValueDto() { }

        public UserCriteriaStepValue ToUserCriteriaStepValue()
        {
            return new UserCriteriaStepValue
            {
                CriteriaStepId = this.CriteriaStepId,
                CriteriaStepValueId = this.CriteriaStepValueId,
                CustomValue = this.CustomValue,
                CriteriaStepValue = this.CriteriaStepValueId.HasValue ? new CriteriaStepValue
                {
                    Id = this.CriteriaStepValueId.Value,
                    Prompt = this.CriteriaStepValuePrompt,
                    Value = this.CriteriaStepValueValue,
                    OrderBy = this.CriteriaStepValueOrderBy
                } : null
            };
        }
    }
}
