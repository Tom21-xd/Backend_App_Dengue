using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("casoreportado")]
    public class Case
    {
        [Key]
        [Column("ID_CASO")]
        [JsonPropertyName("ID_CASO")]
        public int Id { get; set; }

        [Required]
        [Column("DESCRIPCION_CASOREPORTADO")]
        [MaxLength(500)]
        [JsonPropertyName("DESCRIPCION_CASOREPORTADO")]
        public string Description { get; set; } = string.Empty;

        [Column("FECHA_CASOREPORTADO")]
        [JsonPropertyName("FECHA_CASOREPORTADO")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("FK_ID_ESTADOCASO")]
        [JsonPropertyName("FK_ID_ESTADOCASO")]
        public int StateId { get; set; }

        [Column("FK_ID_HOSPITAL")]
        [JsonPropertyName("FK_ID_HOSPITAL")]
        public int HospitalId { get; set; }

        [Column("FK_ID_TIPODENGUE")]
        [JsonPropertyName("FK_ID_TIPODENGUE")]
        public int TypeOfDengueId { get; set; }

        [Column("FK_ID_PACIENTE")]
        [JsonPropertyName("FK_ID_PACIENTE")]
        public int PatientId { get; set; }

        [Column("FK_ID_PERSONALMEDICO")]
        [JsonPropertyName("FK_ID_PERSONALMEDICO")]
        public int? MedicalStaffId { get; set; }

        [Column("FECHAFINALIZACION_CASO")]
        [JsonPropertyName("FECHAFINALIZACION_CASO")]
        public DateTime? FinishedAt { get; set; }

        [Column("DIRECCION_CASOREPORTADO")]
        [MaxLength(255)]
        [JsonPropertyName("DIRECCION_CASOREPORTADO")]
        public string? Address { get; set; }

        [Column("ESTADO_CASO")]
        [JsonPropertyName("ESTADO_CASO")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(StateId))]
        [JsonIgnore]
        public virtual CaseState State { get; set; } = null!;

        [ForeignKey(nameof(HospitalId))]
        [JsonIgnore]
        public virtual Hospital Hospital { get; set; } = null!;

        [ForeignKey(nameof(TypeOfDengueId))]
        [JsonIgnore]
        public virtual TypeOfDengue TypeOfDengue { get; set; } = null!;

        [ForeignKey(nameof(PatientId))]
        [JsonIgnore]
        public virtual User Patient { get; set; } = null!;

        [ForeignKey(nameof(MedicalStaffId))]
        [JsonIgnore]
        public virtual User? MedicalStaff { get; set; }
    }
}
