using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("prevention_categories")]
    public class PreventionCategory
    {
        [Key]
        [Column("ID_CATEGORIA_PREVENCION")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_CATEGORIA")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("DESCRIPCION_CATEGORIA")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("ICONO")]
        [MaxLength(50)]
        public string? Icon { get; set; }

        [Column("COLOR")]
        [MaxLength(20)]
        public string? Color { get; set; }

        [Column("ORDEN_VISUALIZACION")]
        public int DisplayOrder { get; set; }

        [Column("ESTADO_CATEGORIA")]
        public bool IsActive { get; set; } = true;

        [Column("FECHA_CREACION")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<PreventionCategoryImage> Images { get; set; } = new List<PreventionCategoryImage>();

        [JsonIgnore]
        public virtual ICollection<PreventionItem> Items { get; set; } = new List<PreventionItem>();
    }
}
