using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("tipodengue")]
    public class TypeOfDengue
    {
        [Key]
        [Column("ID_TIPODENGUE")]
        [JsonPropertyName("ID_TIPODENGUE")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_TIPODENGUE")]
        [MaxLength(100)]
        [JsonPropertyName("NOMBRE_TIPODENGUE")]
        public string Name { get; set; } = string.Empty;

        [Column("ESTADO_TIPODENGUE")]
        [JsonPropertyName("ESTADO_TIPODENGUE")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();

        [JsonIgnore]
        public virtual ICollection<TypeOfDengueSymptom> TypeOfDengueSymptoms { get; set; } = new List<TypeOfDengueSymptom>();
    }
}
