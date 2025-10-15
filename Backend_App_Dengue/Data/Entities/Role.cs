using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("rol")]
    public class Role
    {
        [Key]
        [Column("ID_ROL")]
        [JsonPropertyName("ID_ROL")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_ROL")]
        [MaxLength(100)]
        [JsonPropertyName("NOMBRE_ROL")]
        public string Name { get; set; } = string.Empty;

        [Column("ESTADO_ROL")]
        [JsonPropertyName("ESTADO_ROL")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
