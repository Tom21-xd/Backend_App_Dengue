using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("certificates")]
    public class Certificate
    {
        [Key]
        [Column("ID_CERTIFICADO")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [Required]
        [Column("FK_ID_INTENTO")]
        public int AttemptId { get; set; }

        [Required]
        [Column("CODIGO_VERIFICACION")]
        [MaxLength(100)]
        public string VerificationCode { get; set; } = string.Empty;

        [Column("FECHA_EMISION")]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [Column("PUNTUACION_OBTENIDA")]
        public decimal Score { get; set; }

        [Column("ESTADO_CERTIFICADO")]
        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Active, Revoked

        [Column("URL_PDF_CERTIFICADO")]
        [MaxLength(500)]
        public string? PdfUrl { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("AttemptId")]
        public virtual QuizAttempt? Attempt { get; set; }
    }
}
