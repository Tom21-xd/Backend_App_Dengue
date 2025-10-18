using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("estado_paciente")]
    public class PatientState
    {
        [Key]
        [Column("ID_ESTADO_PACIENTE")]
        [JsonPropertyName("ID_ESTADO_PACIENTE")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_ESTADO_PACIENTE")]
        [MaxLength(100)]
        [JsonPropertyName("NOMBRE_ESTADO_PACIENTE")]
        public string Name { get; set; } = string.Empty;

        [Column("DESCRIPCION")]
        [MaxLength(255)]
        [JsonPropertyName("DESCRIPCION")]
        public string? Description { get; set; }

        [Required]
        [Column("NIVEL_GRAVEDAD")]
        [JsonPropertyName("NIVEL_GRAVEDAD")]
        public int SeverityLevel { get; set; } // 1=Leve, 2=Moderado, 3=Grave, 4=Crítico, 5=Resuelto

        [Column("COLOR_INDICADOR")]
        [MaxLength(20)]
        [JsonPropertyName("COLOR_INDICADOR")]
        public string? ColorIndicator { get; set; } // Para UI: green, yellow, orange, red, blue

        [Column("ESTADO_ACTIVO")]
        [JsonPropertyName("ESTADO_ACTIVO")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Case> CasesWithCurrentState { get; set; } = new List<Case>();

        [JsonIgnore]
        public virtual ICollection<CaseEvolution> CaseEvolutions { get; set; } = new List<CaseEvolution>();
    }
}
