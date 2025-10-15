using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("tiposangre")]
    public class TypeOfBlood
    {
        [Key]
        [Column("ID_TIPOSANGRE")]
        [JsonPropertyName("ID_TIPOSANGRE")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_TIPOSANGRE")]
        [MaxLength(10)]
        [JsonPropertyName("NOMBRE_TIPOSANGRE")]
        public string Name { get; set; } = string.Empty;

        [Column("ESTADO_TIPOSANGRE")]
        [JsonPropertyName("ESTADO_TIPOSANGRE")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
