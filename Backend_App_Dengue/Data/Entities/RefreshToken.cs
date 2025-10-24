using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    /// <summary>
    /// Entidad para gestionar refresh tokens de usuarios
    /// Permite renovar access tokens sin requerir login constante
    /// </summary>
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("ID_REFRESH_TOKEN")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [Required]
        [Column("TOKEN")]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Column("EXPIRES_AT")]
        public DateTime ExpiresAt { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("IS_REVOKED")]
        public bool IsRevoked { get; set; } = false;

        [Column("REVOKED_AT")]
        public DateTime? RevokedAt { get; set; }

        [Column("DEVICE_INFO")]
        [MaxLength(500)]
        public string? DeviceInfo { get; set; }

        // Navigation property
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
