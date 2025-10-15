using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("notificacion")]
    public class Notification
    {
        [Key]
        [Column("ID_NOTIFICACION")]
        [JsonPropertyName("ID_NOTIFICACION")]
        public int Id { get; set; }

        [Required]
        [Column("CONTENIDO_NOTIFICACION")]
        [MaxLength(500)]
        [JsonPropertyName("CONTENIDO_NOTIFICACION")]
        public string Content { get; set; } = string.Empty;

        [Column("FECHA_NOTIFICACION")]
        [JsonPropertyName("FECHA_NOTIFICACION")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("FK_ID_USUARIO")]
        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [Column("LEIDA_NOTIFICACION")]
        [JsonPropertyName("LEIDA_NOTIFICACION")]
        public bool IsRead { get; set; } = false;

        [Column("ESTADO_NOTIFICACION")]
        [JsonPropertyName("ESTADO_NOTIFICACION")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
