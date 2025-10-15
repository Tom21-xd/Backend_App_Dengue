using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("hospital")]
    public class Hospital
    {
        [Key]
        [Column("ID_HOSPITAL")]
        [JsonPropertyName("ID_HOSPITAL")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_HOSPITAL")]
        [MaxLength(200)]
        [JsonPropertyName("NOMBRE_HOSPITAL")]
        public string Name { get; set; } = string.Empty;

        [Column("DIRECCION_HOSPITAL")]
        [MaxLength(255)]
        [JsonPropertyName("DIRECCION_HOSPITAL")]
        public string? Address { get; set; }

        [Column("FK_ID_MUNICIPIO")]
        [JsonPropertyName("FK_ID_MUNICIPIO")]
        public int CityId { get; set; }

        [Column("LATITUD_HOSPITAL")]
        [MaxLength(50)]
        [JsonPropertyName("LATITUD_HOSPITAL")]
        public string? Latitude { get; set; }

        [Column("LONGITUD_HOSPITAL")]
        [MaxLength(50)]
        [JsonPropertyName("LONGITUD_HOSPITAL")]
        public string? Longitude { get; set; }

        [Column("IMAGEN_HOSPITAL")]
        [MaxLength(255)]
        [JsonPropertyName("IMAGEN_HOSPITAL")]
        public string? ImageId { get; set; }

        [Column("ESTADO_HOSPITAL")]
        [JsonPropertyName("ESTADO_HOSPITAL")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(CityId))]
        [JsonIgnore]
        public virtual City City { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    }
}
