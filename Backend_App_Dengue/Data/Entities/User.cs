using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("usuario")]
    public class User
    {
        [Key]
        [Column("ID_USUARIO")]
        [JsonPropertyName("ID_USUARIO")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_USUARIO")]
        [MaxLength(200)]
        [JsonPropertyName("NOMBRE_USUARIO")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("CORREO_USUARIO")]
        [MaxLength(200)]
        [JsonPropertyName("CORREO_USUARIO")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("CONTRASENIA_USUARIO")]
        [MaxLength(255)]
        [JsonPropertyName("CONTRASENIA_USUARIO")]
        public string Password { get; set; } = string.Empty;

        [Column("DIRECCION_USUARIO")]
        [MaxLength(255)]
        [JsonPropertyName("DIRECCION_USUARIO")]
        public string? Address { get; set; }

        [Column("FECHA_NACIMIENTO_USUARIO")]
        [JsonPropertyName("FECHA_NACIMIENTO_USUARIO")]
        public DateTime? BirthDate { get; set; }

        [Column("FK_ID_ROL")]
        [JsonPropertyName("FK_ID_ROL")]
        public int RoleId { get; set; }

        [Column("FK_ID_MUNICIPIO")]
        [JsonPropertyName("FK_ID_MUNICIPIO")]
        public int? CityId { get; set; }

        [Column("FK_ID_TIPOSANGRE")]
        [JsonPropertyName("FK_ID_TIPOSANGRE")]
        public int BloodTypeId { get; set; }

        [Column("FK_ID_GENERO")]
        [JsonPropertyName("FK_ID_GENERO")]
        public int GenreId { get; set; }

        [Column("ESTADO_USUARIO")]
        [JsonPropertyName("ESTADO_USUARIO")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(RoleId))]
        [JsonIgnore]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey(nameof(CityId))]
        [JsonIgnore]
        public virtual City? City { get; set; }

        [ForeignKey(nameof(BloodTypeId))]
        [JsonIgnore]
        public virtual TypeOfBlood BloodType { get; set; } = null!;

        [ForeignKey(nameof(GenreId))]
        [JsonIgnore]
        public virtual Genre Genre { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<Case> CasesAsPatient { get; set; } = new List<Case>();
        [JsonIgnore]
        public virtual ICollection<Case> CasesAsMedicalStaff { get; set; } = new List<Case>();
        [JsonIgnore]
        public virtual ICollection<Publication> Publications { get; set; } = new List<Publication>();
        [JsonIgnore]
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        [JsonIgnore]
        public virtual ICollection<FCMToken> FCMTokens { get; set; } = new List<FCMToken>();
    }
}
