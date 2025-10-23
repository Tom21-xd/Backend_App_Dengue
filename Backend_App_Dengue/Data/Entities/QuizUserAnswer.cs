using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("quiz_user_answers")]
    public class QuizUserAnswer
    {
        [Key]
        [Column("ID_RESPUESTA_USUARIO")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_INTENTO")]
        public int AttemptId { get; set; }

        [Required]
        [Column("FK_ID_PREGUNTA")]
        public int QuestionId { get; set; }

        [Required]
        [Column("FK_ID_RESPUESTA_SELECCIONADA")]
        public int SelectedAnswerId { get; set; }

        [Column("ES_CORRECTA")]
        public bool IsCorrect { get; set; }

        [Column("FECHA_RESPUESTA")]
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        [Column("TIEMPO_RESPUESTA_SEGUNDOS")]
        public int TimeSpentSeconds { get; set; }

        // Navigation properties
        [ForeignKey("AttemptId")]
        public virtual QuizAttempt? Attempt { get; set; }

        [ForeignKey("QuestionId")]
        public virtual QuizQuestion? Question { get; set; }

        [ForeignKey("SelectedAnswerId")]
        public virtual QuizAnswer? SelectedAnswer { get; set; }
    }
}
