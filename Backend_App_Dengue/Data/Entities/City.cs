using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("municipio")]
    public class City
    {
        [Key]
        [Column("ID_MUNICIPIO")]
        [JsonPropertyName("ID_MUNICIPIO")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_MUNICIPIO")]
        [MaxLength(100)]
        [JsonPropertyName("NOMBRE_MUNICIPIO")]
        public string Name { get; set; } = string.Empty;

        [Column("FK_ID_DEPARTAMENTO")]
        [JsonPropertyName("FK_ID_DEPARTAMENTO")]
        public int DepartmentId { get; set; }

        [Column("ESTADO_MUNICIPIO")]
        [JsonPropertyName("ESTADO_MUNICIPIO")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(DepartmentId))]
        [JsonIgnore]
        public virtual Department Department { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        [JsonIgnore]
        public virtual ICollection<Hospital> Hospitals { get; set; } = new List<Hospital>();
    }
}
