using Bot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.DataAccess.Model
{
    [Table("Users", Schema = "public")]
    [Comment("Таблица пользователей")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Comment("Уникальный идентификатор пользователя")]
        public long Id { get; set; }

        [Column("TelegramId")]
        [Comment("Идентификатор пользователя в Telegram")]
        public long TelegramId { get; set; }

        [Column("State")]
        [Comment("Текущее состояние пользователя")]
        public UserState State { get; set; } = UserState.None;

        [Column("LastUpdated")]
        [Comment("Время последнего обновления")]
        public DateTime? LastUpdated { get; set; }

        [Column("CurrentCriteriaStep")]
        [Comment("Текущий шаг ввода критериев")]
        public int CurrentCriteriaStep { get; set; } = 0;

        [Column("CurrentCriteriaStepValueIndex")]
        [Comment("Текущий индекс значения в CriteriaStepValues")]
        public int CurrentCriteriaStepValueIndex { get; set; } = 0;

        public Criteria? Criteria { get; set; }
    }
}
