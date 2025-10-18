using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("reaccion_publicacion")]
    public class PublicationReaction
    {
        [Key]
        [Column("ID_REACCION")]
        [JsonPropertyName("ID_REACCION")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_PUBLICACION")]
        [JsonPropertyName("FK_ID_PUBLICACION")]
        public int PublicationId { get; set; }

        [Required]
        [Column("FK_ID_USUARIO")]
        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [Required]
        [Column("TIPO_REACCION")]
        [MaxLength(50)]
        [JsonPropertyName("TIPO_REACCION")]
        public string ReactionType { get; set; } = "MeGusta"; // MeGusta, Importante, Util

        [Column("FECHA_REACCION")]
        [JsonPropertyName("FECHA_REACCION")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(PublicationId))]
        [JsonIgnore]
        public virtual Publication Publication { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
