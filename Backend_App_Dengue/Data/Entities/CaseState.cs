using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("estadocaso")]
    public class CaseState
    {
        [Key]
        [Column("ID_ESTADOCASO")]
        [JsonPropertyName("ID_ESTADOCASO")]
        public int Id { get; set; }

        [Required]
        [Column("NOMBRE_ESTADOCASO")]
        [MaxLength(100)]
        [JsonPropertyName("NOMBRE_ESTADOCASO")]
        public string Name { get; set; } = string.Empty;

        [Column("ESTADO_ESTADOCASO")]
        [JsonPropertyName("ESTADO_ESTADOCASO")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    }
}
