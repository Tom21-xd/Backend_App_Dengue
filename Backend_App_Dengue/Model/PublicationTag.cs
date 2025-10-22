using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    [Table("etiqueta")]
    public class PublicationTag
    {
        [Key]
        [Column("ID_ETIQUETA")]
        [JsonPropertyName("ID_ETIQUETA")]
        public int Id { get; set; }

        [Column("NOMBRE_ETIQUETA")]
        [JsonPropertyName("NOMBRE_ETIQUETA")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("ESTADO_ETIQUETA")]
        [JsonPropertyName("ESTADO_ETIQUETA")]
        public bool IsActive { get; set; } = true;
    }
}
