using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("publicacion_etiqueta")]
    public class PublicationTagRelation
    {
        [Key]
        [Column("ID_PUBLICACION_ETIQUETA")]
        [JsonPropertyName("ID_PUBLICACION_ETIQUETA")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_PUBLICACION")]
        [JsonPropertyName("FK_ID_PUBLICACION")]
        public int PublicationId { get; set; }

        [Required]
        [Column("FK_ID_ETIQUETA")]
        [JsonPropertyName("FK_ID_ETIQUETA")]
        public int TagId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(PublicationId))]
        [JsonIgnore]
        public virtual Publication Publication { get; set; } = null!;

        [ForeignKey(nameof(TagId))]
        [JsonIgnore]
        public virtual PublicationTag Tag { get; set; } = null!;
    }
}
