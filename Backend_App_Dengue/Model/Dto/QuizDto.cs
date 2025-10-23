using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO for quiz category
    /// </summary>
    public class QuizCategoryDto
    {
        [JsonPropertyName("ID_CATEGORIA")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_CATEGORIA")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("DESCRIPCION")]
        public string? Description { get; set; }

        [JsonPropertyName("ICONO")]
        public string? Icon { get; set; }

        [JsonPropertyName("ORDEN_VISUALIZACION")]
        public int DisplayOrder { get; set; }

        [JsonPropertyName("ESTA_ACTIVA")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("TOTAL_PREGUNTAS")]
        public int TotalQuestions { get; set; }
    }

    /// <summary>
    /// DTO for quiz question with answers
    /// </summary>
    public class QuizQuestionDto
    {
        [JsonPropertyName("ID_PREGUNTA")]
        public int Id { get; set; }

        [JsonPropertyName("FK_ID_CATEGORIA")]
        public int CategoryId { get; set; }

        [JsonPropertyName("NOMBRE_CATEGORIA")]
        public string? CategoryName { get; set; }

        [JsonPropertyName("TEXTO_PREGUNTA")]
        public string QuestionText { get; set; } = string.Empty;

        [JsonPropertyName("TIPO_PREGUNTA")]
        public string QuestionType { get; set; } = string.Empty;

        [JsonPropertyName("DIFICULTAD")]
        public int Difficulty { get; set; }

        [JsonPropertyName("PUNTOS")]
        public int Points { get; set; }

        [JsonPropertyName("EXPLICACION_RESPUESTA")]
        public string? ExplanationText { get; set; }

        [JsonPropertyName("ESTA_ACTIVA")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("RESPUESTAS")]
        public List<QuizAnswerDto> Answers { get; set; } = new();
    }

    /// <summary>
    /// DTO for quiz answer
    /// </summary>
    public class QuizAnswerDto
    {
        [JsonPropertyName("ID_RESPUESTA")]
        public int Id { get; set; }

        [JsonPropertyName("FK_ID_PREGUNTA")]
        public int QuestionId { get; set; }

        [JsonPropertyName("TEXTO_RESPUESTA")]
        public string AnswerText { get; set; } = string.Empty;

        [JsonPropertyName("ES_CORRECTA")]
        public bool? IsCorrect { get; set; } // Nullable to hide correct answer from user

        [JsonPropertyName("ORDEN_RESPUESTA")]
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// DTO to start a new quiz attempt
    /// </summary>
    public class StartQuizDto
    {
        [JsonPropertyName("ID_USUARIO")]
        public int UserId { get; set; }

        [JsonPropertyName("TOTAL_PREGUNTAS")]
        public int TotalQuestions { get; set; } = 10;
    }

    /// <summary>
    /// DTO for quiz attempt response
    /// </summary>
    public class QuizAttemptDto
    {
        [JsonPropertyName("ID_INTENTO")]
        public int Id { get; set; }

        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [JsonPropertyName("FECHA_INICIO")]
        public DateTime StartedAt { get; set; }

        [JsonPropertyName("PREGUNTAS")]
        public List<QuizQuestionDto> Questions { get; set; } = new();

        [JsonPropertyName("TOTAL_PREGUNTAS")]
        public int TotalQuestions { get; set; }

        [JsonPropertyName("ESTADO_INTENTO")]
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO to submit an answer
    /// </summary>
    public class SubmitAnswerDto
    {
        [JsonPropertyName("ID_INTENTO")]
        public int AttemptId { get; set; }

        [JsonPropertyName("ID_PREGUNTA")]
        public int QuestionId { get; set; }

        [JsonPropertyName("ID_RESPUESTA_SELECCIONADA")]
        public int SelectedAnswerId { get; set; }

        [JsonPropertyName("TIEMPO_RESPUESTA_SEGUNDOS")]
        public int TimeSpentSeconds { get; set; }
    }

    /// <summary>
    /// DTO for answer result feedback
    /// </summary>
    public class AnswerResultDto
    {
        [JsonPropertyName("ES_CORRECTA")]
        public bool IsCorrect { get; set; }

        [JsonPropertyName("RESPUESTA_CORRECTA_ID")]
        public int CorrectAnswerId { get; set; }

        [JsonPropertyName("EXPLICACION")]
        public string? Explanation { get; set; }
    }

    /// <summary>
    /// DTO to submit/finish quiz
    /// </summary>
    public class SubmitQuizDto
    {
        [JsonPropertyName("ID_INTENTO")]
        public int AttemptId { get; set; }

        [JsonPropertyName("TIEMPO_TOTAL_SEGUNDOS")]
        public int TotalTimeSeconds { get; set; }
    }

    /// <summary>
    /// DTO for quiz result
    /// </summary>
    public class QuizResultDto
    {
        [JsonPropertyName("ID_INTENTO")]
        public int AttemptId { get; set; }

        [JsonPropertyName("PUNTUACION_OBTENIDA")]
        public decimal Score { get; set; }

        [JsonPropertyName("TOTAL_PREGUNTAS")]
        public int TotalQuestions { get; set; }

        [JsonPropertyName("RESPUESTAS_CORRECTAS")]
        public int CorrectAnswers { get; set; }

        [JsonPropertyName("RESPUESTAS_INCORRECTAS")]
        public int IncorrectAnswers { get; set; }

        [JsonPropertyName("TIEMPO_TOTAL_SEGUNDOS")]
        public int TotalTimeSeconds { get; set; }

        [JsonPropertyName("FECHA_FINALIZACION")]
        public DateTime CompletedAt { get; set; }

        [JsonPropertyName("APROBADO")]
        public bool Passed { get; set; }

        [JsonPropertyName("PUEDE_GENERAR_CERTIFICADO")]
        public bool CanGenerateCertificate { get; set; }

        [JsonPropertyName("DETALLES_RESPUESTAS")]
        public List<QuizAnswerDetailDto> AnswerDetails { get; set; } = new();
    }

    /// <summary>
    /// DTO for answer detail in result
    /// </summary>
    public class QuizAnswerDetailDto
    {
        [JsonPropertyName("PREGUNTA")]
        public string QuestionText { get; set; } = string.Empty;

        [JsonPropertyName("RESPUESTA_USUARIO")]
        public string UserAnswer { get; set; } = string.Empty;

        [JsonPropertyName("RESPUESTA_CORRECTA")]
        public string CorrectAnswer { get; set; } = string.Empty;

        [JsonPropertyName("ES_CORRECTA")]
        public bool IsCorrect { get; set; }

        [JsonPropertyName("EXPLICACION")]
        public string? Explanation { get; set; }
    }

    /// <summary>
    /// DTO for user's quiz history
    /// </summary>
    public class QuizHistoryDto
    {
        [JsonPropertyName("ID_INTENTO")]
        public int AttemptId { get; set; }

        [JsonPropertyName("FECHA_INICIO")]
        public DateTime StartedAt { get; set; }

        [JsonPropertyName("FECHA_FINALIZACION")]
        public DateTime? CompletedAt { get; set; }

        [JsonPropertyName("PUNTUACION_OBTENIDA")]
        public decimal Score { get; set; }

        [JsonPropertyName("TOTAL_PREGUNTAS")]
        public int TotalQuestions { get; set; }

        [JsonPropertyName("RESPUESTAS_CORRECTAS")]
        public int CorrectAnswers { get; set; }

        [JsonPropertyName("ESTADO_INTENTO")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("APROBADO")]
        public bool Passed { get; set; }

        [JsonPropertyName("TIENE_CERTIFICADO")]
        public bool HasCertificate { get; set; }
    }

    /// <summary>
    /// DTO for certificate
    /// </summary>
    public class CertificateDto
    {
        [JsonPropertyName("ID_CERTIFICADO")]
        public int Id { get; set; }

        [JsonPropertyName("CODIGO_VERIFICACION")]
        public string VerificationCode { get; set; } = string.Empty;

        [JsonPropertyName("FECHA_EMISION")]
        public DateTime IssuedAt { get; set; }

        [JsonPropertyName("PUNTUACION_OBTENIDA")]
        public decimal Score { get; set; }

        [JsonPropertyName("NOMBRE_USUARIO")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("CORREO_USUARIO")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonPropertyName("URL_PDF_CERTIFICADO")]
        public string? PdfUrl { get; set; }

        [JsonPropertyName("ESTADO_CERTIFICADO")]
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO to create/edit question (Admin)
    /// </summary>
    public class CreateQuizQuestionDto
    {
        [JsonPropertyName("FK_ID_CATEGORIA")]
        public int CategoryId { get; set; }

        [JsonPropertyName("TEXTO_PREGUNTA")]
        public string QuestionText { get; set; } = string.Empty;

        [JsonPropertyName("TIPO_PREGUNTA")]
        public string QuestionType { get; set; } = "MultipleChoice";

        [JsonPropertyName("DIFICULTAD")]
        public int Difficulty { get; set; } = 2;

        [JsonPropertyName("PUNTOS")]
        public int Points { get; set; } = 10;

        [JsonPropertyName("EXPLICACION_RESPUESTA")]
        public string? ExplanationText { get; set; }

        [JsonPropertyName("ESTA_ACTIVA")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("RESPUESTAS")]
        public List<CreateQuizAnswerDto> Answers { get; set; } = new();
    }

    /// <summary>
    /// DTO to create answer (Admin)
    /// </summary>
    public class CreateQuizAnswerDto
    {
        [JsonPropertyName("TEXTO_RESPUESTA")]
        public string AnswerText { get; set; } = string.Empty;

        [JsonPropertyName("ES_CORRECTA")]
        public bool IsCorrect { get; set; }

        [JsonPropertyName("ORDEN_RESPUESTA")]
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// DTO to create/edit category (Admin)
    /// </summary>
    public class CreateQuizCategoryDto
    {
        [JsonPropertyName("NOMBRE_CATEGORIA")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("DESCRIPCION")]
        public string? Description { get; set; }

        [JsonPropertyName("ICONO")]
        public string? Icon { get; set; }

        [JsonPropertyName("ORDEN_VISUALIZACION")]
        public int DisplayOrder { get; set; }

        [JsonPropertyName("ESTA_ACTIVA")]
        public bool IsActive { get; set; } = true;
    }
}
