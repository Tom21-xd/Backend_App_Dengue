using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("departamento")]
    public class Department
    {
        [Key]
        [Column("ID_DEPARTAMENTO")]
        [JsonPropertyName("ID_DEPARTAMENTO")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_DEPARTAMENTO")]
        [MaxLength(100)]
        [JsonPropertyName("NOMBRE_DEPARTAMENTO")]
        public string Name { get; set; } = string.Empty;

        [Column("ESTADO_DEPARTAMENTO")]
        [JsonPropertyName("ESTADO_DEPARTAMENTO")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<City> Cities { get; set; } = new List<City>();
    }
}
