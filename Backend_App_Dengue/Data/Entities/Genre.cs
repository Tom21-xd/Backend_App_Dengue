using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("genero")]
    public class Genre
    {
        [Key]
        [Column("ID_GENERO")]
        [JsonPropertyName("ID_GENERO")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_GENERO")]
        [MaxLength(50)]
        [JsonPropertyName("NOMBRE_GENERO")]
        public string Name { get; set; } = string.Empty;

        [Column("ESTADO_GENERO")]
        [JsonPropertyName("ESTADO_GENERO")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
