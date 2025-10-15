using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("publicacion")]
    public class Publication
    {
        [Key]
        [Column("ID_PUBLICACION")]
        [JsonPropertyName("ID_PUBLICACION")]
        public int Id { get; set; }

        [Required]
        [Column("TITULO_PUBLICACION")]
        [MaxLength(200)]
        [JsonPropertyName("TITULO_PUBLICACION")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("DESCRIPCION_PUBLICACION")]
        [MaxLength(1000)]
        [JsonPropertyName("DESCRIPCION_PUBLICACION")]
        public string Description { get; set; } = string.Empty;

        [Column("FECHA_PUBLICACION")]
        [JsonPropertyName("FECHA_PUBLICACION")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("FK_ID_USUARIO")]
        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        // MongoDB GridFS Image ID
        [Column("ID_IMAGEN")]
        [MaxLength(255)]
        [JsonPropertyName("ID_IMAGEN")]
        public string? ImageId { get; set; }

        [Column("ESTADO_PUBLICACION")]
        [JsonPropertyName("ESTADO_PUBLICACION")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
