using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.DataAccess.Model
{
    [Table("Критерии")]
    public class Criteria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("Пользователь")]
        public long UserId { get; set; }

        [Column("Регион")]
        public string Region { get; set; }

        [Column("Должность")]
        public string Post { get; set; }

        [Column("Зарплата")]
        public decimal? Salary { get; set; }

        [Column("Опыт работ")]
        public int? Experience { get; set; }

        [Column("Тип занятости")]
        public string? WorkType { get; set; }

        [Column("График работы")]
        public string? Schedule { get; set; }

        [Column("Доступные людям с инвалидностью")]
        public bool Disability { get; set; } = false;

        [Column("Доступные с 14 лет")]
        public bool ForChildren { get; set; } = false;
    }
}
