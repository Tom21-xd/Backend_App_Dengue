using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("lectura_publicacion")]
    public class PublicationView
    {
        [Key]
        [Column("ID_LECTURA")]
        [JsonPropertyName("ID_LECTURA")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_PUBLICACION")]
        [JsonPropertyName("FK_ID_PUBLICACION")]
        public int PublicationId { get; set; }

        [Required]
        [Column("FK_ID_USUARIO")]
        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [Column("FECHA_LECTURA")]
        [JsonPropertyName("FECHA_LECTURA")]
        public DateTime ViewedAt { get; set; } = DateTime.Now;

        [Column("TIEMPO_LECTURA_SEGUNDOS")]
        [JsonPropertyName("TIEMPO_LECTURA_SEGUNDOS")]
        public int? ReadingTimeSeconds { get; set; }

        // Navigation properties
        [ForeignKey(nameof(PublicationId))]
        [JsonIgnore]
        public virtual Publication Publication { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
