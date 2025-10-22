using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    [Table("categoria_publicacion")]
    public class PublicationCategory
    {
        [Key]
        [Column("ID_CATEGORIA_PUBLICACION")]
        [JsonPropertyName("ID_CATEGORIA_PUBLICACION")]
        public int Id { get; set; }

        [Column("NOMBRE_CATEGORIA")]
        [JsonPropertyName("NOMBRE_CATEGORIA")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("DESCRIPCION_CATEGORIA")]
        [JsonPropertyName("DESCRIPCION_CATEGORIA")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("ICONO")]
        [JsonPropertyName("ICONO")]
        [StringLength(50)]
        public string? Icon { get; set; }

        [Column("COLOR")]
        [JsonPropertyName("COLOR")]
        [StringLength(50)]
        public string? Color { get; set; }

        [Column("ESTADO_CATEGORIA")]
        [JsonPropertyName("ESTADO_CATEGORIA")]
        public bool IsActive { get; set; } = true;
    }
}
