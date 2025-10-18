using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("evolucion_caso")]
    public class CaseEvolution
    {
        [Key]
        [Column("ID_EVOLUCION")]
        [JsonPropertyName("ID_EVOLUCION")]
        public int Id { get; set; }

        // Foreign Keys
        [Required]
        [Column("FK_ID_CASO")]
        [JsonPropertyName("FK_ID_CASO")]
        public int CaseId { get; set; }

        [Required]
        [Column("FK_ID_MEDICO")]
        [JsonPropertyName("FK_ID_MEDICO")]
        public int DoctorId { get; set; }

        [Required]
        [Column("FK_ID_TIPODENGUE")]
        [JsonPropertyName("FK_ID_TIPODENGUE")]
        public int TypeOfDengueId { get; set; }

        [Required]
        [Column("FK_ID_ESTADO_PACIENTE")]
        [JsonPropertyName("FK_ID_ESTADO_PACIENTE")]
        public int PatientStateId { get; set; }

        // Fecha y día de enfermedad
        [Required]
        [Column("FECHA_EVOLUCION")]
        [JsonPropertyName("FECHA_EVOLUCION")]
        public DateTime EvolutionDate { get; set; }

        [Column("DIA_ENFERMEDAD")]
        [JsonPropertyName("DIA_ENFERMEDAD")]
        public int? DayOfIllness { get; set; }

        // Síntomas reportados (JSON array de IDs)
        [Required]
        [Column("SINTOMAS_REPORTADOS", TypeName = "json")]
        [JsonPropertyName("SINTOMAS_REPORTADOS")]
        public string ReportedSymptomsJson { get; set; } = "[]";

        // Signos vitales
        [Column("TEMPERATURA")]
        [JsonPropertyName("TEMPERATURA")]
        public decimal? Temperature { get; set; }

        [Column("PRESION_ARTERIAL_SISTOLICA")]
        [JsonPropertyName("PRESION_ARTERIAL_SISTOLICA")]
        public int? SystolicBloodPressure { get; set; }

        [Column("PRESION_ARTERIAL_DIASTOLICA")]
        [JsonPropertyName("PRESION_ARTERIAL_DIASTOLICA")]
        public int? DiastolicBloodPressure { get; set; }

        [Column("FRECUENCIA_CARDIACA")]
        [JsonPropertyName("FRECUENCIA_CARDIACA")]
        public int? HeartRate { get; set; }

        [Column("FRECUENCIA_RESPIRATORIA")]
        [JsonPropertyName("FRECUENCIA_RESPIRATORIA")]
        public int? RespiratoryRate { get; set; }

        [Column("SATURACION_OXIGENO")]
        [JsonPropertyName("SATURACION_OXIGENO")]
        public decimal? OxygenSaturation { get; set; }

        // Laboratorios
        [Column("PLAQUETAS")]
        [JsonPropertyName("PLAQUETAS")]
        public int? Platelets { get; set; }

        [Column("HEMATOCRITO")]
        [JsonPropertyName("HEMATOCRITO")]
        public decimal? Hematocrit { get; set; }

        [Column("LEUCOCITOS")]
        [JsonPropertyName("LEUCOCITOS")]
        public int? WhiteBloodCells { get; set; }

        [Column("HEMOGLOBINA")]
        [JsonPropertyName("HEMOGLOBINA")]
        public decimal? Hemoglobin { get; set; }

        [Column("TRANSAMINASAS_AST")]
        [JsonPropertyName("TRANSAMINASAS_AST")]
        public int? AST { get; set; }

        [Column("TRANSAMINASAS_ALT")]
        [JsonPropertyName("TRANSAMINASAS_ALT")]
        public int? ALT { get; set; }

        // Evaluación clínica
        [Column("OBSERVACIONES_CLINICAS", TypeName = "TEXT")]
        [JsonPropertyName("OBSERVACIONES_CLINICAS")]
        public string? ClinicalObservations { get; set; }

        [Column("TRATAMIENTO_INDICADO", TypeName = "TEXT")]
        [JsonPropertyName("TRATAMIENTO_INDICADO")]
        public string? PrescribedTreatment { get; set; }

        [Column("EXAMENES_SOLICITADOS", TypeName = "TEXT")]
        [JsonPropertyName("EXAMENES_SOLICITADOS")]
        public string? RequestedTests { get; set; }

        // Alertas y cambios
        [Column("CAMBIO_TIPO_DENGUE")]
        [JsonPropertyName("CAMBIO_TIPO_DENGUE")]
        public bool DengueTypeChanged { get; set; } = false;

        [Column("EMPEORAMIENTO_DETECTADO")]
        [JsonPropertyName("EMPEORAMIENTO_DETECTADO")]
        public bool DeteriorationDetected { get; set; } = false;

        [Column("REQUIERE_HOSPITALIZACION")]
        [JsonPropertyName("REQUIERE_HOSPITALIZACION")]
        public bool RequiresHospitalization { get; set; } = false;

        [Column("REQUIERE_UCI")]
        [JsonPropertyName("REQUIERE_UCI")]
        public bool RequiresICU { get; set; } = false;

        // Seguimiento
        [Column("PROXIMA_CITA")]
        [JsonPropertyName("PROXIMA_CITA")]
        public DateTime? NextAppointment { get; set; }

        [Column("RECOMENDACIONES_PACIENTE", TypeName = "TEXT")]
        [JsonPropertyName("RECOMENDACIONES_PACIENTE")]
        public string? PatientRecommendations { get; set; }

        // Metadata
        [Column("ESTADO_EVOLUCION")]
        [JsonPropertyName("ESTADO_EVOLUCION")]
        public bool IsActive { get; set; } = true;

        [Column("FECHA_REGISTRO")]
        [JsonPropertyName("FECHA_REGISTRO")]
        public DateTime CreatedAt { get; set; }

        [Column("FECHA_MODIFICACION")]
        [JsonPropertyName("FECHA_MODIFICACION")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        [ForeignKey("CaseId")]
        public virtual Case Case { get; set; } = null!;

        [JsonIgnore]
        [ForeignKey("DoctorId")]
        public virtual User Doctor { get; set; } = null!;

        [JsonIgnore]
        [ForeignKey("TypeOfDengueId")]
        public virtual TypeOfDengue TypeOfDengue { get; set; } = null!;

        [JsonIgnore]
        [ForeignKey("PatientStateId")]
        public virtual PatientState PatientState { get; set; } = null!;
    }
}
