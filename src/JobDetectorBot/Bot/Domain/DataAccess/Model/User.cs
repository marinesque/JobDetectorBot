using Bot.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.DataAccess.Model
{
    [Table("Пользователь")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("From.Id из Telegram")]
        public long TelegramId { get; set; }

        [Column("Статус")]
        public UserState State { get; set; } = UserState.None; // Состояние сценария

        [Column("Дата последнего обновления")]

        public DateTime? LastUpdated;

        public Criteria? Criteria { get; set; }

        public int CurrentCriteriaStep { get; set; } = -1; // Текущий шаг ввода критериев
    }
}
