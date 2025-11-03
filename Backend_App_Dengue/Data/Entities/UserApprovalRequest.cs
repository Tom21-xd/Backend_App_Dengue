using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("solicitud_aprobacion_usuario")]
    public class UserApprovalRequest
    {
        [Key]
        [Column("ID_SOLICITUD")]
        [JsonPropertyName("ID_SOLICITUD")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_USUARIO")]
        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [Required]
        [Column("ESTADO_SOLICITUD")]
        [MaxLength(20)]
        [JsonPropertyName("ESTADO_SOLICITUD")]
        public string Status { get; set; } = "PENDIENTE"; // PENDIENTE, APROBADO, RECHAZADO

        [Required]
        [Column("ROL_SOLICITADO")]
        [JsonPropertyName("ROL_SOLICITADO")]
        public int RequestedRoleId { get; set; }

        [Column("MOTIVO_RECHAZO")]
        [MaxLength(500)]
        [JsonPropertyName("MOTIVO_RECHAZO")]
        public string? RejectionReason { get; set; }

        [Column("FK_ID_ADMIN")]
        [JsonPropertyName("FK_ID_ADMIN")]
        public int? ApprovedByAdminId { get; set; }

        [Required]
        [Column("FECHA_SOLICITUD")]
        [JsonPropertyName("FECHA_SOLICITUD")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Column("FECHA_RESOLUCION")]
        [JsonPropertyName("FECHA_RESOLUCION")]
        public DateTime? ResolutionDate { get; set; }

        [Column("DATOS_RETHUS")]
        [MaxLength(1000)]
        [JsonPropertyName("DATOS_RETHUS")]
        public string? RethusData { get; set; } // JSON con los datos enviados a RETHUS

        [Column("ERROR_RETHUS")]
        [MaxLength(500)]
        [JsonPropertyName("ERROR_RETHUS")]
        public string? RethusError { get; set; } // Descripción del error cuando RETHUS falló

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(RequestedRoleId))]
        [JsonIgnore]
        public virtual Role RequestedRole { get; set; } = null!;

        [ForeignKey(nameof(ApprovedByAdminId))]
        [JsonIgnore]
        public virtual User? ApprovedByAdmin { get; set; }
    }
}
