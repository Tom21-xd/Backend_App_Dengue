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
        public int? HospitalId { get; set; }

        [Column("FK_ID_TIPODENGUE")]
        [JsonPropertyName("FK_ID_TIPODENGUE")]
        public int TypeOfDengueId { get; set; }

        [Column("FK_ID_PACIENTE")]
        [JsonPropertyName("FK_ID_PACIENTE")]
        public int? PatientId { get; set; }

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

        // ========================================
        // CAMPOS EPIDEMIOLÓGICOS (Importación CSV)
        // ========================================

        /// <summary>
        /// Año del reporte epidemiológico
        /// </summary>
        [Column("ANIO_REPORTE")]
        [JsonPropertyName("ANIO_REPORTE")]
        public int? Year { get; set; }

        /// <summary>
        /// Edad del paciente (para casos sin usuario registrado)
        /// </summary>
        [Column("EDAD_PACIENTE")]
        [JsonPropertyName("EDAD_PACIENTE")]
        public int? Age { get; set; }

        /// <summary>
        /// Nombre temporal para casos anónimos o no registrados
        /// </summary>
        [Column("NOMBRE_TEMPORAL")]
        [MaxLength(200)]
        [JsonPropertyName("NOMBRE_TEMPORAL")]
        public string? TemporaryName { get; set; }

        /// <summary>
        /// Barrio o Vereda donde se reportó el caso
        /// </summary>
        [Column("BARRIO_VEREDA")]
        [MaxLength(255)]
        [JsonPropertyName("BARRIO_VEREDA")]
        public string? Neighborhood { get; set; }

        /// <summary>
        /// Latitud del lugar del reporte (formato ISO)
        /// </summary>
        [Column("LATITUD")]
        [JsonPropertyName("LATITUD")]
        public decimal? Latitude { get; set; }

        /// <summary>
        /// Longitud del lugar del reporte (formato ISO)
        /// </summary>
        [Column("LONGITUD")]
        [JsonPropertyName("LONGITUD")]
        public decimal? Longitude { get; set; }

        /// <summary>
        /// Usuario que registró el caso (personal médico o admin)
        /// </summary>
        [Column("FK_ID_USUARIO_REGISTRO")]
        [JsonPropertyName("FK_ID_USUARIO_REGISTRO")]
        public int? RegisteredByUserId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(StateId))]
        [JsonIgnore]
        public virtual CaseState State { get; set; } = null!;

        [ForeignKey(nameof(HospitalId))]
        [JsonIgnore]
        public virtual Hospital? Hospital { get; set; }

        [ForeignKey(nameof(TypeOfDengueId))]
        [JsonIgnore]
        public virtual TypeOfDengue TypeOfDengue { get; set; } = null!;

        [ForeignKey(nameof(PatientId))]
        [JsonIgnore]
        public virtual User? Patient { get; set; }

        [ForeignKey(nameof(MedicalStaffId))]
        [JsonIgnore]
        public virtual User? MedicalStaff { get; set; }

        [ForeignKey(nameof(RegisteredByUserId))]
        [JsonIgnore]
        public virtual User? RegisteredBy { get; set; }
    }
}
