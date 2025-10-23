using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("quiz_attempts")]
    public class QuizAttempt
    {
        [Key]
        [Column("ID_INTENTO")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [Column("FECHA_INICIO")]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        [Column("FECHA_FINALIZACION")]
        public DateTime? CompletedAt { get; set; }

        [Column("PUNTUACION_OBTENIDA")]
        public decimal Score { get; set; } = 0;

        [Column("TOTAL_PREGUNTAS")]
        public int TotalQuestions { get; set; }

        [Column("RESPUESTAS_CORRECTAS")]
        public int CorrectAnswers { get; set; } = 0;

        [Column("RESPUESTAS_INCORRECTAS")]
        public int IncorrectAnswers { get; set; } = 0;

        [Column("TIEMPO_TOTAL_SEGUNDOS")]
        public int TotalTimeSeconds { get; set; } = 0;

        [Column("ESTADO_INTENTO")]
        [MaxLength(50)]
        public string Status { get; set; } = "InProgress"; // InProgress, Completed, Abandoned

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<QuizUserAnswer> UserAnswers { get; set; } = new List<QuizUserAnswer>();
        public virtual Certificate? Certificate { get; set; }
    }
}
