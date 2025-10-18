using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("categoria_publicacion")]
    public class PublicationCategory
    {
        [Key]
        [Column("ID_CATEGORIA_PUBLICACION")]
        [JsonPropertyName("ID_CATEGORIA_PUBLICACION")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_CATEGORIA")]
        [MaxLength(100)]
        [JsonPropertyName("NOMBRE_CATEGORIA")]
        public string Name { get; set; } = string.Empty;

        [Column("DESCRIPCION_CATEGORIA")]
        [MaxLength(255)]
        [JsonPropertyName("DESCRIPCION_CATEGORIA")]
        public string? Description { get; set; }

        [Column("ICONO")]
        [MaxLength(50)]
        [JsonPropertyName("ICONO")]
        public string? Icon { get; set; }

        [Column("COLOR")]
        [MaxLength(20)]
        [JsonPropertyName("COLOR")]
        public string? Color { get; set; }

        [Column("ESTADO_CATEGORIA")]
        [JsonPropertyName("ESTADO_CATEGORIA")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Publication> Publications { get; set; } = new List<Publication>();
    }
}
