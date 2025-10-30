using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    /// <summary>
    /// Representa un permiso individual en el sistema
    /// </summary>
    [Table("permiso")]
    public class Permission
    {
        [Key]
        [Column("ID_PERMISO")]
        [JsonPropertyName("ID_PERMISO")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_PERMISO")]
        [MaxLength(100)]
        [JsonPropertyName("NOMBRE_PERMISO")]
        public string Name { get; set; } = string.Empty;

        [Column("DESCRIPCION_PERMISO")]
        [MaxLength(255)]
        [JsonPropertyName("DESCRIPCION_PERMISO")]
        public string? Description { get; set; }

        [Required]
        [Column("CODIGO_PERMISO")]
        [MaxLength(100)]
        [JsonPropertyName("CODIGO_PERMISO")]
        public string Code { get; set; } = string.Empty;

        [Column("CATEGORIA_PERMISO")]
        [MaxLength(50)]
        [JsonPropertyName("CATEGORIA_PERMISO")]
        public string? Category { get; set; }

        [Column("ESTADO_PERMISO")]
        [JsonPropertyName("ESTADO_PERMISO")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
