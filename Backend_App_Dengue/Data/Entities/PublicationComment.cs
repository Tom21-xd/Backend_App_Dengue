using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("comentario_publicacion")]
    public class PublicationComment
    {
        [Key]
        [Column("ID_COMENTARIO")]
        [JsonPropertyName("ID_COMENTARIO")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_PUBLICACION")]
        [JsonPropertyName("FK_ID_PUBLICACION")]
        public int PublicationId { get; set; }

        [Required]
        [Column("FK_ID_USUARIO")]
        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [Required]
        [Column("CONTENIDO_COMENTARIO", TypeName = "TEXT")]
        [JsonPropertyName("CONTENIDO_COMENTARIO")]
        public string Content { get; set; } = string.Empty;

        [Column("FK_ID_COMENTARIO_PADRE")]
        [JsonPropertyName("FK_ID_COMENTARIO_PADRE")]
        public int? ParentCommentId { get; set; }

        [Column("FECHA_COMENTARIO")]
        [JsonPropertyName("FECHA_COMENTARIO")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("ESTADO_COMENTARIO")]
        [JsonPropertyName("ESTADO_COMENTARIO")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(PublicationId))]
        [JsonIgnore]
        public virtual Publication Publication { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(ParentCommentId))]
        [JsonIgnore]
        public virtual PublicationComment? ParentComment { get; set; }

        [JsonIgnore]
        public virtual ICollection<PublicationComment> Replies { get; set; } = new List<PublicationComment>();
    }
}
