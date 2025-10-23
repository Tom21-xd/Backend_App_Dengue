using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("quiz_categories")]
    public class QuizCategory
    {
        [Key]
        [Column("ID_CATEGORIA")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_CATEGORIA")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("DESCRIPCION")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("ICONO")]
        [MaxLength(50)]
        public string? Icon { get; set; }

        [Column("ORDEN_VISUALIZACION")]
        public int DisplayOrder { get; set; }

        [Column("ESTADO_CATEGORIA")]
        public bool IsActive { get; set; } = true;

        [Column("FECHA_CREACION")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }
}
