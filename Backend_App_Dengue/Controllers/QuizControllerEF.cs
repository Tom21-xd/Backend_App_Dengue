using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class QuizControllerEF : ControllerBase
    {
        private readonly AppDbContext _context;
        private const decimal PASSING_SCORE = 80.0m;

        public QuizControllerEF(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all active quiz categories
        /// </summary>
        [HttpGet("categories")]
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
        /// Start a new quiz attempt
        /// Returns random questions distributed across categories
        /// </summary>
        [HttpPost("start")]
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
        /// Submit an answer to a question
        /// </summary>
        [HttpPost("answer")]
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
        /// Submit/finish the quiz and calculate score
        /// </summary>
        [HttpPost("submit")]
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
        /// Get quiz result by attempt ID
        /// </summary>
        [HttpGet("result/{attemptId}")]
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
        /// Get user's quiz history
        /// </summary>
        [HttpGet("history/{userId}")]
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
