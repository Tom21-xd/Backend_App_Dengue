using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("tipodengue_sintoma")]
    public class TypeOfDengueSymptom
    {
        [Key]
        [Column("ID_TIPODENGUE_SINTOMA")]
        [JsonPropertyName("ID_TIPODENGUE_SINTOMA")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_TIPODENGUE")]
        [JsonPropertyName("FK_ID_TIPODENGUE")]
        public int TypeOfDengueId { get; set; }

        [Required]
        [Column("FK_ID_SINTOMA")]
        [JsonPropertyName("FK_ID_SINTOMA")]
        public int SymptomId { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual TypeOfDengue TypeOfDengue { get; set; } = null!;

        [JsonIgnore]
        public virtual Symptom Symptom { get; set; } = null!;
    }
}
