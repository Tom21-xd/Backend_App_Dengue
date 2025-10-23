using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_App_Dengue.Data.Entities
{
    [Table("quiz_questions")]
    public class QuizQuestion
    {
        [Key]
        [Column("ID_PREGUNTA")]
        public int Id { get; set; }

        [Required]
        [Column("FK_ID_CATEGORIA")]
        public int CategoryId { get; set; }

        [Required]
        [Column("TEXTO_PREGUNTA")]
        [MaxLength(1000)]
        public string QuestionText { get; set; } = string.Empty;

        [Column("TIPO_PREGUNTA")]
        [MaxLength(50)]
        public string QuestionType { get; set; } = "MultipleChoice"; // MultipleChoice, TrueFalse

        [Column("DIFICULTAD")]
        public int Difficulty { get; set; } = 2; // 1=Fácil, 2=Medio, 3=Difícil

        [Column("PUNTOS")]
        public int Points { get; set; } = 10;

        [Column("EXPLICACION_RESPUESTA")]
        [MaxLength(1000)]
        public string? ExplanationText { get; set; }

        [Column("ESTADO_PREGUNTA")]
        public bool IsActive { get; set; } = true;

        [Column("FECHA_CREACION")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("FECHA_MODIFICACION")]
        public DateTime? ModifiedAt { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual QuizCategory? Category { get; set; }

        public virtual ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
        public virtual ICollection<QuizUserAnswer> UserAnswers { get; set; } = new List<QuizUserAnswer>();
    }
}
