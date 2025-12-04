using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("prevention_item_images")]
    public class PreventionItemImage
    {
        [Key]
        [Column("ID_IMAGEN_ITEM")]
        public int Id { get; set; }

        [Column("FK_ID_ITEM_PREVENCION")]
        public int ItemId { get; set; }

        [Required]
        [Column("ID_IMAGEN_MONGO")]
        [MaxLength(255)]
        public string MongoImageId { get; set; } = string.Empty;

        [Column("TITULO_IMAGEN")]
        [MaxLength(100)]
        public string? Title { get; set; }

        [Column("ORDEN_VISUALIZACION")]
        public int DisplayOrder { get; set; }

        [Column("FECHA_CREACION")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(ItemId))]
        [JsonIgnore]
        public virtual PreventionItem Item { get; set; } = null!;
    }
}
