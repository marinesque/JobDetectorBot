using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Domain.DataAccess.Model
{
    [Table("UserCriteriaStepValues", Schema = "public")]
    [Comment("Таблица значений критериев пользователя")]
    [Index(nameof(UserId), nameof(CriteriaStepId), IsUnique = false)]
    public class UserCriteriaStepValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Comment("Уникальный идентификатор")]
        public long Id { get; set; }

        [Column("UserId")]
        [Comment("Идентификатор пользователя")]
        public long UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Column("CriteriaStepId")]
        [Comment("Идентификатор шага критерия")]
        public long CriteriaStepId { get; set; }

        [ForeignKey("CriteriaStepId")]
        public CriteriaStep CriteriaStep { get; set; }

        [Column("CriteriaStepValueId")]
        [Comment("Идентификатор значения шага критерия (если выбрано из списка)")]
        public long? CriteriaStepValueId { get; set; }

        [ForeignKey("CriteriaStepValueId")]
        public CriteriaStepValue? CriteriaStepValue { get; set; }

        [Column("CustomValue", TypeName = "text")]
        [Comment("Пользовательское значение (если введено вручную)")]
        public string? CustomValue { get; set; }

        [Column("CreatedAt")]
        [Comment("Дата создания записи")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        [Comment("Дата последнего обновления")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}