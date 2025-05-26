using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.DataAccess.Model
{
    [Table("CriteriaStepValues", Schema = "public")]
    [Comment("Таблица значений шагов критериев")]
    [Index(nameof(CriteriaStepId), nameof(Value), IsUnique = true)]
    public class CriteriaStepValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Comment("Уникальный идентификатор значения шага критерия")]
        public long Id { get; set; }

        [Column("CriteriaStepId")]
        [Comment("Идентификатор шага критерия")]
        public long CriteriaStepId { get; set; }

        [ForeignKey("CriteriaStepId")]
        public CriteriaStep CriteriaStep { get; set; }

        [Column("Prompt")]
        [Comment("Отображаемое значение шага критерия")]
        public string Prompt { get; set; }

        [Column("Value")]
        [Comment("Значение шага критерия")]
        public string Value { get; set; }

        [Column("OrderBy")]
        [Comment("Порядок сортировки значения")]
        public int? OrderBy { get; set; }
    }
}