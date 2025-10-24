using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    /// <summary>
    /// Controlador para gestión del sistema de evaluación (Quiz) sobre prevención del dengue
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class QuizControllerEF : ControllerBase
    {
        private readonly AppDbContext _context;
        private const decimal PASSING_SCORE = 80.0m;

        public QuizControllerEF(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todas las categorías activas del quiz
        /// </summary>
        /// <returns>Lista de categorías con el total de preguntas de cada una</returns>
        /// <response code="200">Categorías obtenidas exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<QuizCategoryDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<QuizCategoryDto>>> GetCategories()
        {
            try
            {
                var categories = await _context.QuizCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new QuizCategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Icon = c.Icon,
                        DisplayOrder = c.DisplayOrder,
                        TotalQuestions = c.Questions.Count(q => q.IsActive)
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener categorías", error = ex.Message });
            }
        }

        /// <summary>
        /// Inicia un nuevo intento de quiz para un usuario
        /// </summary>
        /// <param name="request">Datos de inicio del quiz (ID de usuario y cantidad de preguntas)</param>
        /// <returns>Intento de quiz con preguntas aleatorias distribuidas entre categorías</returns>
        /// <response code="200">Quiz iniciado exitosamente con preguntas asignadas</response>
        /// <response code="400">Solicitud inválida o no hay preguntas disponibles</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Este endpoint selecciona preguntas aleatorias de diferentes categorías,
        /// distribuye las preguntas equitativamente y crea un registro de intento en estado "InProgress".
        /// NO muestra cuál es la respuesta correcta en las opciones.
        /// </remarks>
        [HttpPost("start")]
        [ProducesResponseType(typeof(QuizAttemptDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        public async Task<ActionResult<QuizAttemptDto>> StartQuiz([FromBody] StartQuizDto request)
        {
            try
            {
                // Validate user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Get active categories
                var categories = await _context.QuizCategories
                    .Where(c => c.IsActive)
                    .Include(c => c.Questions.Where(q => q.IsActive))
                        .ThenInclude(q => q.Answers)
                    .ToListAsync();

                if (!categories.Any() || categories.All(c => !c.Questions.Any()))
                {
                    return BadRequest(new { message = "No hay preguntas disponibles" });
                }

                // Select random questions distributed across categories
                var selectedQuestions = new List<QuizQuestion>();
                var questionsPerCategory = Math.Max(1, request.TotalQuestions / categories.Count);
                var random = new Random();

                foreach (var category in categories)
                {
                    var categoryQuestions = category.Questions
                        .OrderBy(x => random.Next())
                        .Take(questionsPerCategory)
                        .ToList();

                    selectedQuestions.AddRange(categoryQuestions);
                }

                // If we don't have enough, get more random questions
                if (selectedQuestions.Count < request.TotalQuestions)
                {
                    var allQuestions = categories.SelectMany(c => c.Questions).ToList();
                    var additionalNeeded = request.TotalQuestions - selectedQuestions.Count;
                    var additional = allQuestions
                        .Where(q => !selectedQuestions.Contains(q))
                        .OrderBy(x => random.Next())
                        .Take(additionalNeeded);

                    selectedQuestions.AddRange(additional);
                }

                // Take exact number requested and shuffle
                selectedQuestions = selectedQuestions
                    .OrderBy(x => random.Next())
                    .Take(request.TotalQuestions)
                    .ToList();

                // Create quiz attempt
                var attempt = new QuizAttempt
                {
                    UserId = request.UserId,
                    StartedAt = DateTime.UtcNow,
                    TotalQuestions = selectedQuestions.Count,
                    Status = "InProgress"
                };

                _context.QuizAttempts.Add(attempt);
                await _context.SaveChangesAsync();

                // Store question IDs for this attempt (we'll store them with first answer)
                // For now, return questions in DTO

                var response = new QuizAttemptDto
                {
                    Id = attempt.Id,
                    UserId = attempt.UserId,
                    StartedAt = attempt.StartedAt,
                    TotalQuestions = selectedQuestions.Count,
                    Status = attempt.Status,
                    Questions = selectedQuestions.Select(q => new QuizQuestionDto
                    {
                        Id = q.Id,
                        CategoryId = q.CategoryId,
                        CategoryName = q.Category?.Name,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Difficulty = q.Difficulty,
                        Points = q.Points,
                        Answers = q.Answers
                            .OrderBy(a => a.DisplayOrder)
                            .Select(a => new QuizAnswerDto
                            {
                                Id = a.Id,
                                QuestionId = a.QuestionId,
                                AnswerText = a.AnswerText,
                                DisplayOrder = a.DisplayOrder,
                                IsCorrect = null // Hide correct answer from client
                            }).ToList()
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al iniciar quiz", error = ex.Message });
            }
        }

        /// <summary>
        /// Registra la respuesta del usuario a una pregunta del quiz
        /// </summary>
        /// <param name="request">Datos de la respuesta (ID de intento, pregunta, respuesta seleccionada y tiempo)</param>
        /// <returns>Resultado indicando si la respuesta fue correcta, la respuesta correcta y la explicación</returns>
        /// <response code="200">Respuesta registrada exitosamente</response>
        /// <response code="400">Solicitud inválida, quiz finalizado o pregunta ya respondida</response>
        /// <response code="404">Intento o pregunta no encontrada</response>
        /// <response code="500">Error interno del servidor o pregunta sin respuesta correcta configurada</response>
        /// <remarks>
        /// Valida que el intento exista y esté en estado "InProgress",
        /// que la pregunta no haya sido respondida previamente,
        /// y que la respuesta seleccionada pertenezca a la pregunta.
        /// </remarks>
        [HttpPost("answer")]
        [ProducesResponseType(typeof(AnswerResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        public async Task<ActionResult<AnswerResultDto>> SubmitAnswer([FromBody] SubmitAnswerDto request)
        {
            try
            {
                // Validate attempt
                var attempt = await _context.QuizAttempts.FindAsync(request.AttemptId);
                if (attempt == null)
                {
                    return NotFound(new { message = "Intento no encontrado" });
                }

                if (attempt.Status != "InProgress")
                {
                    return BadRequest(new { message = "El quiz ya fue finalizado" });
                }

                // Get question and correct answer
                var question = await _context.QuizQuestions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == request.QuestionId);

                if (question == null)
                {
                    return NotFound(new { message = "Pregunta no encontrada" });
                }

                var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == request.SelectedAnswerId);
                if (selectedAnswer == null)
                {
                    return BadRequest(new { message = "Respuesta no válida" });
                }

                var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
                if (correctAnswer == null)
                {
                    return StatusCode(500, new { message = "Pregunta sin respuesta correcta configurada" });
                }

                // Check if already answered
                var existingAnswer = await _context.QuizUserAnswers
                    .FirstOrDefaultAsync(ua => ua.AttemptId == request.AttemptId && ua.QuestionId == request.QuestionId);

                if (existingAnswer != null)
                {
                    return BadRequest(new { message = "Esta pregunta ya fue respondida" });
                }

                // Save user answer
                var userAnswer = new QuizUserAnswer
                {
                    AttemptId = request.AttemptId,
                    QuestionId = request.QuestionId,
                    SelectedAnswerId = request.SelectedAnswerId,
                    IsCorrect = selectedAnswer.IsCorrect,
                    TimeSpentSeconds = request.TimeSpentSeconds,
                    AnsweredAt = DateTime.UtcNow
                };

                _context.QuizUserAnswers.Add(userAnswer);
                await _context.SaveChangesAsync();

                // Return result
                return Ok(new AnswerResultDto
                {
                    IsCorrect = selectedAnswer.IsCorrect,
                    CorrectAnswerId = correctAnswer.Id,
                    Explanation = question.ExplanationText
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al guardar respuesta", error = ex.Message });
            }
        }

        /// <summary>
        /// Finaliza el quiz y calcula la puntuación final del usuario
        /// </summary>
        /// <param name="request">Datos de finalización (ID de intento y tiempo total en segundos)</param>
        /// <returns>Resultado completo del quiz con puntuación, respuestas correctas/incorrectas y detalles</returns>
        /// <response code="200">Quiz finalizado y calificado exitosamente</response>
        /// <response code="400">Solicitud inválida o quiz ya finalizado previamente</response>
        /// <response code="404">Intento de quiz no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Calcula: Puntuación = (respuestas correctas / total preguntas) * 100.
        /// Aprobado si puntuación >= 80%.
        /// El quiz cambia a estado "Completed" y no puede ser modificado después.
        /// </remarks>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(QuizResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        public async Task<ActionResult<QuizResultDto>> SubmitQuiz([FromBody] SubmitQuizDto request)
        {
            try
            {
                var attempt = await _context.QuizAttempts
                    .Include(a => a.UserAnswers)
                        .ThenInclude(ua => ua.Question)
                    .Include(a => a.UserAnswers)
                        .ThenInclude(ua => ua.SelectedAnswer)
                    .FirstOrDefaultAsync(a => a.Id == request.AttemptId);

                if (attempt == null)
                {
                    return NotFound(new { message = "Intento no encontrado" });
                }

                if (attempt.Status != "InProgress")
                {
                    return BadRequest(new { message = "El quiz ya fue finalizado" });
                }

                // Calculate results
                var correctAnswers = attempt.UserAnswers.Count(ua => ua.IsCorrect);
                var incorrectAnswers = attempt.UserAnswers.Count(ua => !ua.IsCorrect);
                var score = attempt.TotalQuestions > 0
                    ? (decimal)correctAnswers / attempt.TotalQuestions * 100
                    : 0;

                // Update attempt
                attempt.CompletedAt = DateTime.UtcNow;
                attempt.Score = score;
                attempt.CorrectAnswers = correctAnswers;
                attempt.IncorrectAnswers = incorrectAnswers;
                attempt.TotalTimeSeconds = request.TotalTimeSeconds;
                attempt.Status = "Completed";

                await _context.SaveChangesAsync();

                // Get answer details
                var answerDetails = new List<QuizAnswerDetailDto>();
                foreach (var userAnswer in attempt.UserAnswers)
                {
                    var correctAnswer = await _context.QuizAnswers
                        .FirstOrDefaultAsync(a => a.QuestionId == userAnswer.QuestionId && a.IsCorrect);

                    answerDetails.Add(new QuizAnswerDetailDto
                    {
                        QuestionText = userAnswer.Question?.QuestionText ?? "",
                        UserAnswer = userAnswer.SelectedAnswer?.AnswerText ?? "",
                        CorrectAnswer = correctAnswer?.AnswerText ?? "",
                        IsCorrect = userAnswer.IsCorrect,
                        Explanation = userAnswer.Question?.ExplanationText
                    });
                }

                var passed = score >= PASSING_SCORE;

                return Ok(new QuizResultDto
                {
                    AttemptId = attempt.Id,
                    Score = score,
                    TotalQuestions = attempt.TotalQuestions,
                    CorrectAnswers = correctAnswers,
                    IncorrectAnswers = incorrectAnswers,
                    TotalTimeSeconds = request.TotalTimeSeconds,
                    CompletedAt = attempt.CompletedAt.Value,
                    Passed = passed,
                    CanGenerateCertificate = passed,
                    AnswerDetails = answerDetails
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al finalizar quiz", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el resultado detallado de un intento de quiz completado
        /// </summary>
        /// <param name="attemptId">ID del intento de quiz</param>
        /// <returns>Resultado del quiz con puntuación, detalles de respuestas y estado de aprobación</returns>
        /// <response code="200">Resultado obtenido exitosamente</response>
        /// <response code="400">El quiz aún no ha sido finalizado</response>
        /// <response code="404">Intento de quiz no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("result/{attemptId}")]
        [ProducesResponseType(typeof(QuizResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<QuizResultDto>> GetResult(int attemptId)
        {
            try
            {
                var attempt = await _context.QuizAttempts
                    .Include(a => a.UserAnswers)
                        .ThenInclude(ua => ua.Question)
                    .Include(a => a.UserAnswers)
                        .ThenInclude(ua => ua.SelectedAnswer)
                    .FirstOrDefaultAsync(a => a.Id == attemptId);

                if (attempt == null)
                {
                    return NotFound(new { message = "Intento no encontrado" });
                }

                if (attempt.Status != "Completed")
                {
                    return BadRequest(new { message = "El quiz aún no ha sido finalizado" });
                }

                // Get answer details
                var answerDetails = new List<QuizAnswerDetailDto>();
                foreach (var userAnswer in attempt.UserAnswers)
                {
                    var correctAnswer = await _context.QuizAnswers
                        .FirstOrDefaultAsync(a => a.QuestionId == userAnswer.QuestionId && a.IsCorrect);

                    answerDetails.Add(new QuizAnswerDetailDto
                    {
                        QuestionText = userAnswer.Question?.QuestionText ?? "",
                        UserAnswer = userAnswer.SelectedAnswer?.AnswerText ?? "",
                        CorrectAnswer = correctAnswer?.AnswerText ?? "",
                        IsCorrect = userAnswer.IsCorrect,
                        Explanation = userAnswer.Question?.ExplanationText
                    });
                }

                return Ok(new QuizResultDto
                {
                    AttemptId = attempt.Id,
                    Score = attempt.Score,
                    TotalQuestions = attempt.TotalQuestions,
                    CorrectAnswers = attempt.CorrectAnswers,
                    IncorrectAnswers = attempt.IncorrectAnswers,
                    TotalTimeSeconds = attempt.TotalTimeSeconds,
                    CompletedAt = attempt.CompletedAt ?? DateTime.UtcNow,
                    Passed = attempt.Score >= PASSING_SCORE,
                    CanGenerateCertificate = attempt.Score >= PASSING_SCORE,
                    AnswerDetails = answerDetails
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener resultado", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el historial de intentos de quiz de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de todos los intentos de quiz del usuario ordenados por fecha (más reciente primero)</returns>
        /// <response code="200">Historial obtenido exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Incluye información sobre fecha de inicio/finalización, puntuación obtenida,
        /// estado del intento (InProgress, Completed), si aprobó (>= 80%) y si tiene certificado generado.
        /// </remarks>
        [HttpGet("history/{userId}")]
        [ProducesResponseType(typeof(List<QuizHistoryDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<QuizHistoryDto>>> GetHistory(int userId)
        {
            try
            {
                var attempts = await _context.QuizAttempts
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.StartedAt)
                    .Select(a => new QuizHistoryDto
                    {
                        AttemptId = a.Id,
                        StartedAt = a.StartedAt,
                        CompletedAt = a.CompletedAt,
                        Score = a.Score,
                        TotalQuestions = a.TotalQuestions,
                        CorrectAnswers = a.CorrectAnswers,
                        Status = a.Status,
                        Passed = a.Score >= PASSING_SCORE,
                        HasCertificate = a.Certificate != null
                    })
                    .ToListAsync();

                return Ok(attempts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener historial", error = ex.Message });
            }
        }
    }
}
