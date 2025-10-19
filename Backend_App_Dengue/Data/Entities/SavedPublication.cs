using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("publicacion_guardada")]
    public class SavedPublication
    {
        [Key]
        [Column("ID_GUARDADO")]
        [JsonPropertyName("ID_GUARDADO")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_PUBLICACION")]
        [JsonPropertyName("FK_ID_PUBLICACION")]
        public int PublicationId { get; set; }

        [Required]
        [Column("FK_ID_USUARIO")]
        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [Column("FECHA_GUARDADO")]
        [JsonPropertyName("FECHA_GUARDADO")]
        public DateTime SavedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(PublicationId))]
        [JsonPropertyName("PUBLICACION")]
        public virtual Publication Publication { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
