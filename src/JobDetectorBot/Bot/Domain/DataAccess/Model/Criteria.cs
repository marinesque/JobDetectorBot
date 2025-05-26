using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.DataAccess.Model
{
    [Table("Criteria", Schema = "public")]
    [Comment("Таблица критериев")]
    public class Criteria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Comment("Уникальный идентификатор критерия")]
        public long Id { get; set; }

        [Column("UserId")]
        [Comment("Идентификатор пользователя")]
        public long UserId { get; set; }

        [Column("Region")]
        [Comment("Регион")]
        public string? Region { get; set; }

        [Column("Post")]
        [Comment("Должность")]
        public string? Post { get; set; }

        [Column("Salary")]
        [Comment("Зарплата")]
        public decimal? Salary { get; set; }

        [Column("Experience")]
        [Comment("Опыт работы")]
        public int? Experience { get; set; }

        [Column("WorkType")]
        [Comment("Тип занятости")]
        public string? WorkType { get; set; }

        [Column("Schedule")]
        [Comment("График работы")]
        public string? Schedule { get; set; }

        [Column("Disability")]
        [Comment("Доступно для людей с инвалидностью")]
        public bool Disability { get; set; } = false;

        [Column("ForChildren")]
        [Comment("Доступно для детей с 14 лет")]
        public bool ForChildren { get; set; } = false;
    }
}
