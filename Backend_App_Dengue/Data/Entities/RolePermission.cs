using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    /// <summary>
    /// Tabla intermedia para relación Many-to-Many entre Roles y Permisos
    /// </summary>
    [Table("rol_permiso")]
    public class RolePermission
    {
        [Key]
        [Column("ID_ROL_PERMISO")]
        [JsonPropertyName("ID_ROL_PERMISO")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_ROL")]
        [JsonPropertyName("FK_ID_ROL")]
        public int RoleId { get; set; }

        [Required]
        [Column("FK_ID_PERMISO")]
        [JsonPropertyName("FK_ID_PERMISO")]
        public int PermissionId { get; set; }

        [Column("FECHA_ASIGNACION")]
        [JsonPropertyName("FECHA_ASIGNACION")]
        public DateTime AssignedAt { get; set; } = DateTime.Now;

        [Column("ESTADO_ROL_PERMISO")]
        [JsonPropertyName("ESTADO_ROL_PERMISO")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(RoleId))]
        [JsonIgnore]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey(nameof(PermissionId))]
        [JsonIgnore]
        public virtual Permission Permission { get; set; } = null!;
    }
}
