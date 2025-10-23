using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("quiz_answers")]
    public class QuizAnswer
    {
        [Key]
        [Column("ID_RESPUESTA")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_PREGUNTA")]
        public int QuestionId { get; set; }

        [Required]
        [Column("TEXTO_RESPUESTA")]
        [MaxLength(500)]
        public string AnswerText { get; set; } = string.Empty;

        [Column("ES_CORRECTA")]
        public bool IsCorrect { get; set; } = false;

        [Column("ORDEN_RESPUESTA")]
        public int DisplayOrder { get; set; }

        // Navigation properties
        [ForeignKey("QuestionId")]
        public virtual QuizQuestion? Question { get; set; }

        public virtual ICollection<QuizUserAnswer> UserAnswers { get; set; } = new List<QuizUserAnswer>();
    }
}
