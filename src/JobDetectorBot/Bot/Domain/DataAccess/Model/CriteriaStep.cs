using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.DataAccess.Model
{
    [Table("CriteriaStep", Schema = "public")]
    [Comment("Таблица шагов критериев")]
    [Index(nameof(Name), IsUnique = true)]
    public class CriteriaStep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Comment("Уникальный идентификатор шага критерия")]
        public long Id { get; set; }

        [Column("Name")]
        [Comment("Название шага")]
        public string Name { get; set; }

        [Column("Prompt")]
        [Comment("Отображаемое значение для шага")]
        public string Prompt { get; set; }

        [Column("IsCustom")]
        [Comment("Возможно ли кастомное значение")]
        public bool IsCustom { get; set; } = true;

        [Column("OrderBy")]
        [Comment("Порядок сортировки")]
        public int OrderBy { get; set; } = 0;

        [Column("Type")]
        [Comment("Тип значения")]
        public string Type { get; set; }

        [Column("IsMapped")]
        [Comment("Маппинг с сервисом вакансий")]
        public bool IsMapped { get; set; } = false;

        [Column("MainDictionary", TypeName = "jsonb")]
        [Comment("Основной словарь значений")]
        public string? MainDictionary { get; set; }

        public List<CriteriaStepValue>? CriteriaStepValues { get; set; } = new();
    }
}