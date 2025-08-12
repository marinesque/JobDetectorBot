using Bot.Domain.DataAccess.Model;
using Bot.Domain.Enums;

namespace Bot.Domain.DataAccess.Dto
{
    public class UserCacheDto
    {
        public long TelegramId { get; set; }
        public UserState State { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int CurrentCriteriaStep { get; set; }
        public int CurrentCriteriaStepValueIndex { get; set; }
        public bool IsSingle { get; set; }
        public List<UserCriteriaStepValueDto> UserCriteriaStepValues { get; set; } = new();

        public static UserCacheDto FromUser(User user)
        {
            return new UserCacheDto
            {
                TelegramId = user.TelegramId,
                State = user.State,
                LastUpdated = user.LastUpdated,
                CurrentCriteriaStep = user.CurrentCriteriaStep,
                CurrentCriteriaStepValueIndex = user.CurrentCriteriaStepValueIndex,
                IsSingle = user.IsSingle,
                UserCriteriaStepValues = user.UserCriteriaStepValues?
                    .Select(v => new UserCriteriaStepValueDto
                    {
                        CriteriaStepId = v.CriteriaStepId,
                        CriteriaStepValueId = v.CriteriaStepValueId,
                        CustomValue = v.CustomValue,
                        CriteriaStepValuePrompt = v.CriteriaStepValue?.Prompt,
                        CriteriaStepValueValue = v.CriteriaStepValue?.Value,
                        CriteriaStepValueOrderBy = v.CriteriaStepValue?.OrderBy
                    })
                    .ToList() ?? new List<UserCriteriaStepValueDto>()
            };
        }

        public User ToUser()
        {
            return new User
            {
                TelegramId = this.TelegramId,
                State = this.State,
                LastUpdated = this.LastUpdated,
                CurrentCriteriaStep = this.CurrentCriteriaStep,
                CurrentCriteriaStepValueIndex = this.CurrentCriteriaStepValueIndex,
                IsSingle = this.IsSingle,
                UserCriteriaStepValues = this.UserCriteriaStepValues
                .Select(v => v.ToUserCriteriaStepValue())
                .ToList()
            };
        }
    }
}
