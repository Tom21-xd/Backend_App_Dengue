using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("etiqueta_publicacion")]
    public class PublicationTag
    {
        [Key]
        [Column("ID_ETIQUETA")]
        [JsonPropertyName("ID_ETIQUETA")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_ETIQUETA")]
        [MaxLength(50)]
        [JsonPropertyName("NOMBRE_ETIQUETA")]
        public string Name { get; set; } = string.Empty;

        [Column("ESTADO_ETIQUETA")]
        [JsonPropertyName("ESTADO_ETIQUETA")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<PublicationTagRelation> PublicationRelations { get; set; } = new List<PublicationTagRelation>();
    }
}
