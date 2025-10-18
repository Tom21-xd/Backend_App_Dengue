using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("sintoma")]
    public class Symptom
    {
        [Key]
        [Column("ID_SINTOMA")]
        [JsonPropertyName("ID_SINTOMA")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_SINTOMA")]
        [MaxLength(200)]
        [JsonPropertyName("NOMBRE_SINTOMA")]
        public string Name { get; set; } = string.Empty;

        [Column("ESTADO_SINTOMA")]
        [JsonPropertyName("ESTADO_SINTOMA")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<TypeOfDengueSymptom> TypeOfDengueSymptoms { get; set; } = new List<TypeOfDengueSymptom>();
    }
}
