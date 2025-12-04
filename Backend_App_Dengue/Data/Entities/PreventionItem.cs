using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("prevention_items")]
    public class PreventionItem
    {
        [Key]
        [Column("ID_ITEM_PREVENCION")]
        public int Id { get; set; }

        [Column("FK_ID_CATEGORIA_PREVENCION")]
        public int CategoryId { get; set; }

        [Required]
        [Column("TITULO_ITEM")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("DESCRIPCION_ITEM")]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Column("EMOJI_ITEM")]
        [MaxLength(10)]
        public string? Emoji { get; set; }

        [Column("ES_ADVERTENCIA")]
        public bool IsWarning { get; set; } = false;

        [Column("ORDEN_VISUALIZACION")]
        public int DisplayOrder { get; set; }

        [Column("ESTADO_ITEM")]
        public bool IsActive { get; set; } = true;

        [Column("FECHA_CREACION")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(CategoryId))]
        [JsonIgnore]
        public virtual PreventionCategory Category { get; set; } = null!;

        public virtual ICollection<PreventionItemImage> Images { get; set; } = new List<PreventionItemImage>();
    }
}
