using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class QuizStatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuizStatisticsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get overall quiz statistics
        /// </summary>
        [HttpGet("overview")]
        public async Task<ActionResult<QuizOverviewStatsDto>> GetOverviewStatistics()
        {
            try
            {
                var totalAttempts = await _context.QuizAttempts.CountAsync();
                var completedAttempts = await _context.QuizAttempts.CountAsync(a => a.Status == "Completed");
                var inProgressAttempts = await _context.QuizAttempts.CountAsync(a => a.Status == "InProgress");
                var totalCertificates = await _context.Certificates.CountAsync();

                var averageScore = await _context.QuizAttempts
                    .Where(a => a.Status == "Completed")
                    .AverageAsync(a => (double?)a.Score) ?? 0;

                var passRate = completedAttempts > 0
                    ? await _context.QuizAttempts
                        .Where(a => a.Status == "Completed" && a.Score >= 80)
                        .CountAsync() * 100.0 / completedAttempts
                    : 0;

                var totalQuestions = await _context.QuizQuestions.CountAsync();
                var activeQuestions = await _context.QuizQuestions.CountAsync(q => q.IsActive);
                var totalCategories = await _context.QuizCategories.CountAsync();

                var stats = new QuizOverviewStatsDto
                {
                    TotalAttempts = totalAttempts,
                    CompletedAttempts = completedAttempts,
                    InProgressAttempts = inProgressAttempts,
                    TotalCertificatesIssued = totalCertificates,
                    AverageScore = Math.Round(averageScore, 2),
                    PassRate = Math.Round(passRate, 2),
                    TotalQuestions = totalQuestions,
                    ActiveQuestions = activeQuestions,
                    TotalCategories = totalCategories
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas generales", error = ex.Message });
            }
        }

        /// <summary>
        /// Get statistics by category
        /// </summary>
        [HttpGet("by-category")]
        public async Task<ActionResult<List<CategoryStatsDto>>> GetCategoryStatistics()
        {
            try
            {
                var categories = await _context.QuizCategories
                    .Include(c => c.Questions)
                        .ThenInclude(q => q.UserAnswers)
                    .ToListAsync();

                var stats = categories.Select(c =>
                {
                    var totalAnswers = c.Questions.SelectMany(q => q.UserAnswers).Count();
                    var correctAnswers = c.Questions.SelectMany(q => q.UserAnswers).Count(ua => ua.IsCorrect);

                    return new CategoryStatsDto
                    {
                        CategoryId = c.Id,
                        CategoryName = c.Name,
                        TotalQuestions = c.Questions.Count,
                        ActiveQuestions = c.Questions.Count(q => q.IsActive),
                        TimesAnswered = totalAnswers,
                        CorrectAnswers = correctAnswers,
                        AccuracyRate = totalAnswers > 0 ? Math.Round(correctAnswers * 100.0 / totalAnswers, 2) : 0
                    };
                }).ToList();

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas por categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Get statistics for a specific question
        /// </summary>
        [HttpGet("question/{questionId}")]
        public async Task<ActionResult<QuestionStatsDto>> GetQuestionStatistics(int questionId)
        {
            try
            {
                var question = await _context.QuizQuestions
                    .Include(q => q.Category)
                    .Include(q => q.Answers)
                        .ThenInclude(a => a.UserAnswers)
                    .Include(q => q.UserAnswers)
                    .FirstOrDefaultAsync(q => q.Id == questionId);

                if (question == null)
                {
                    return NotFound(new { message = "Pregunta no encontrada" });
                }

                var totalAnswers = question.UserAnswers.Count;
                var correctAnswers = question.UserAnswers.Count(ua => ua.IsCorrect);
                var incorrectAnswers = totalAnswers - correctAnswers;

                var answerDistribution = question.Answers.Select(a => new AnswerDistributionDto
                {
                    AnswerId = a.Id,
                    AnswerText = a.AnswerText,
                    IsCorrect = a.IsCorrect,
                    TimesSelected = a.UserAnswers.Count,
                    SelectionPercentage = totalAnswers > 0
                        ? Math.Round(a.UserAnswers.Count * 100.0 / totalAnswers, 2)
                        : 0
                }).ToList();

                var stats = new QuestionStatsDto
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    CategoryName = question.Category?.Name ?? "",
                    Difficulty = question.Difficulty,
                    TotalTimesAnswered = totalAnswers,
                    CorrectAnswers = correctAnswers,
                    IncorrectAnswers = incorrectAnswers,
                    AccuracyRate = totalAnswers > 0 ? Math.Round(correctAnswers * 100.0 / totalAnswers, 2) : 0,
                    AnswerDistribution = answerDistribution
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas de pregunta", error = ex.Message });
            }
        }

        /// <summary>
        /// Get top performing users
        /// </summary>
        [HttpGet("top-users")]
        public async Task<ActionResult<List<UserPerformanceDto>>> GetTopUsers([FromQuery] int limit = 10)
        {
            try
            {
                var userStats = await _context.QuizAttempts
                    .Where(a => a.Status == "Completed")
                    .GroupBy(a => a.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        TotalAttempts = g.Count(),
                        AverageScore = g.Average(a => a.Score),
                        BestScore = g.Max(a => a.Score),
                        PassedCount = g.Count(a => a.Score >= 80)
                    })
                    .OrderByDescending(x => x.AverageScore)
                    .Take(limit)
                    .ToListAsync();

                var result = new List<UserPerformanceDto>();
                foreach (var stat in userStats)
                {
                    var user = await _context.Users.FindAsync(stat.UserId);
                    if (user != null)
                    {
                        result.Add(new UserPerformanceDto
                        {
                            UserId = stat.UserId,
                            UserName = user.Name,
                            UserEmail = user.Email,
                            TotalAttempts = stat.TotalAttempts,
                            AverageScore = Math.Round(stat.AverageScore, 2),
                            BestScore = stat.BestScore,
                            PassedCount = stat.PassedCount,
                            PassRate = Math.Round(stat.PassedCount * 100.0 / stat.TotalAttempts, 2)
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener usuarios destacados", error = ex.Message });
            }
        }

        /// <summary>
        /// Get recent quiz attempts
        /// </summary>
        [HttpGet("recent-attempts")]
        public async Task<ActionResult<List<RecentAttemptDto>>> GetRecentAttempts([FromQuery] int limit = 20)
        {
            try
            {
                var attempts = await _context.QuizAttempts
                    .Include(a => a.User)
                    .OrderByDescending(a => a.StartedAt)
                    .Take(limit)
                    .Select(a => new RecentAttemptDto
                    {
                        AttemptId = a.Id,
                        UserId = a.UserId,
                        UserName = a.User!.Name,
                        StartedAt = a.StartedAt,
                        CompletedAt = a.CompletedAt,
                        Score = a.Score,
                        Status = a.Status,
                        TotalQuestions = a.TotalQuestions,
                        CorrectAnswers = a.CorrectAnswers,
                        Passed = a.Score >= 80
                    })
                    .ToListAsync();

                return Ok(attempts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener intentos recientes", error = ex.Message });
            }
        }

        /// <summary>
        /// Get quiz attempts by date range
        /// </summary>
        [HttpGet("attempts-by-date")]
        public async Task<ActionResult<List<AttemptsByDateDto>>> GetAttemptsByDate(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var attempts = await _context.QuizAttempts
                    .Where(a => a.StartedAt >= start && a.StartedAt <= end)
                    .GroupBy(a => a.StartedAt.Date)
                    .Select(g => new AttemptsByDateDto
                    {
                        Date = g.Key,
                        TotalAttempts = g.Count(),
                        CompletedAttempts = g.Count(a => a.Status == "Completed"),
                        PassedAttempts = g.Count(a => a.Score >= 80),
                        AverageScore = Math.Round(g.Where(a => a.Status == "Completed").Average(a => (double?)a.Score) ?? 0, 2)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                return Ok(attempts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener intentos por fecha", error = ex.Message });
            }
        }

        /// <summary>
        /// Get most difficult questions (lowest accuracy)
        /// </summary>
        [HttpGet("difficult-questions")]
        public async Task<ActionResult<List<DifficultQuestionDto>>> GetDifficultQuestions([FromQuery] int limit = 10)
        {
            try
            {
                var questionStats = await _context.QuizQuestions
                    .Include(q => q.Category)
                    .Include(q => q.UserAnswers)
                    .Where(q => q.UserAnswers.Any())
                    .Select(q => new
                    {
                        Question = q,
                        TotalAnswers = q.UserAnswers.Count,
                        CorrectAnswers = q.UserAnswers.Count(ua => ua.IsCorrect),
                        AccuracyRate = q.UserAnswers.Count > 0
                            ? q.UserAnswers.Count(ua => ua.IsCorrect) * 100.0 / q.UserAnswers.Count
                            : 0
                    })
                    .OrderBy(x => x.AccuracyRate)
                    .Take(limit)
                    .ToListAsync();

                var result = questionStats.Select(qs => new DifficultQuestionDto
                {
                    QuestionId = qs.Question.Id,
                    QuestionText = qs.Question.QuestionText,
                    CategoryName = qs.Question.Category?.Name ?? "",
                    Difficulty = qs.Question.Difficulty,
                    TotalAnswers = qs.TotalAnswers,
                    CorrectAnswers = qs.CorrectAnswers,
                    AccuracyRate = Math.Round(qs.AccuracyRate, 2)
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener preguntas difíciles", error = ex.Message });
            }
        }

        /// <summary>
        /// Get completion rate over time
        /// </summary>
        [HttpGet("completion-rate")]
        public async Task<ActionResult<CompletionRateDto>> GetCompletionRate()
        {
            try
            {
                var totalAttempts = await _context.QuizAttempts.CountAsync();
                var completedAttempts = await _context.QuizAttempts.CountAsync(a => a.Status == "Completed");
                var inProgressAttempts = await _context.QuizAttempts.CountAsync(a => a.Status == "InProgress");
                var abandonedAttempts = await _context.QuizAttempts.CountAsync(a => a.Status == "Abandoned");

                var completionRate = totalAttempts > 0
                    ? Math.Round(completedAttempts * 100.0 / totalAttempts, 2)
                    : 0;

                var averageCompletionTime = await _context.QuizAttempts
                    .Where(a => a.Status == "Completed" && a.CompletedAt.HasValue)
                    .Select(a => (a.CompletedAt!.Value - a.StartedAt).TotalMinutes)
                    .AverageAsync();

                var result = new CompletionRateDto
                {
                    TotalAttempts = totalAttempts,
                    CompletedAttempts = completedAttempts,
                    InProgressAttempts = inProgressAttempts,
                    AbandonedAttempts = abandonedAttempts,
                    CompletionRate = completionRate,
                    AverageCompletionTimeMinutes = Math.Round(averageCompletionTime, 2)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener tasa de finalización", error = ex.Message });
            }
        }

        /// <summary>
        /// Get certificate statistics
        /// </summary>
        [HttpGet("certificates")]
        public async Task<ActionResult<CertificateStatsDto>> GetCertificateStatistics()
        {
            try
            {
                var totalCertificates = await _context.Certificates.CountAsync();
                var activeCertificates = await _context.Certificates.CountAsync(c => c.Status == "Active");
                var revokedCertificates = await _context.Certificates.CountAsync(c => c.Status == "Revoked");

                var certificatesThisMonth = await _context.Certificates
                    .CountAsync(c => c.IssuedAt.Month == DateTime.UtcNow.Month &&
                                     c.IssuedAt.Year == DateTime.UtcNow.Year);

                var certificatesThisYear = await _context.Certificates
                    .CountAsync(c => c.IssuedAt.Year == DateTime.UtcNow.Year);

                var averageScoreForCertificate = await _context.Certificates
                    .AverageAsync(c => (double?)c.Score) ?? 0;

                var result = new CertificateStatsDto
                {
                    TotalCertificates = totalCertificates,
                    ActiveCertificates = activeCertificates,
                    RevokedCertificates = revokedCertificates,
                    CertificatesThisMonth = certificatesThisMonth,
                    CertificatesThisYear = certificatesThisYear,
                    AverageScore = Math.Round(averageScoreForCertificate, 2)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas de certificados", error = ex.Message });
            }
        }
    }

    #region Statistics DTOs

    public class QuizOverviewStatsDto
    {
        public int TotalAttempts { get; set; }
        public int CompletedAttempts { get; set; }
        public int InProgressAttempts { get; set; }
        public int TotalCertificatesIssued { get; set; }
        public double AverageScore { get; set; }
        public double PassRate { get; set; }
        public int TotalQuestions { get; set; }
        public int ActiveQuestions { get; set; }
        public int TotalCategories { get; set; }
    }

    public class CategoryStatsDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int ActiveQuestions { get; set; }
        public int TimesAnswered { get; set; }
        public int CorrectAnswers { get; set; }
        public double AccuracyRate { get; set; }
    }

    public class QuestionStatsDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public int TotalTimesAnswered { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public double AccuracyRate { get; set; }
        public List<AnswerDistributionDto> AnswerDistribution { get; set; } = new();
    }

    public class AnswerDistributionDto
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int TimesSelected { get; set; }
        public double SelectionPercentage { get; set; }
    }

    public class UserPerformanceDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public decimal AverageScore { get; set; }
        public decimal BestScore { get; set; }
        public int PassedCount { get; set; }
        public double PassRate { get; set; }
    }

    public class RecentAttemptDto
    {
        public int AttemptId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal Score { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public bool Passed { get; set; }
    }

    public class AttemptsByDateDto
    {
        public DateTime Date { get; set; }
        public int TotalAttempts { get; set; }
        public int CompletedAttempts { get; set; }
        public int PassedAttempts { get; set; }
        public double AverageScore { get; set; }
    }

    public class DifficultQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public double AccuracyRate { get; set; }
    }

    public class CompletionRateDto
    {
        public int TotalAttempts { get; set; }
        public int CompletedAttempts { get; set; }
        public int InProgressAttempts { get; set; }
        public int AbandonedAttempts { get; set; }
        public double CompletionRate { get; set; }
        public double AverageCompletionTimeMinutes { get; set; }
    }

    public class CertificateStatsDto
    {
        public int TotalCertificates { get; set; }
        public int ActiveCertificates { get; set; }
        public int RevokedCertificates { get; set; }
        public int CertificatesThisMonth { get; set; }
        public int CertificatesThisYear { get; set; }
        public double AverageScore { get; set; }
    }

    #endregion
}
