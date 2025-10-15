using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("fcmtoken")]
    public class FCMToken
    {
        [Key]
        [Column("ID_FCMTOKEN")]
        [JsonPropertyName("ID_FCMTOKEN")]
        public int Id { get; set; }

        [Column("FK_ID_USUARIO")]
        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [Required]
        [Column("FCM_TOKEN")]
        [MaxLength(500)]
        [JsonPropertyName("FCM_TOKEN")]
        public string Token { get; set; } = string.Empty;

        [Column("FECHA_REGISTRO")]
        [JsonPropertyName("FECHA_REGISTRO")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("FECHA_ACTUALIZACION")]
        [JsonPropertyName("FECHA_ACTUALIZACION")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
