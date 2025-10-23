using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class QuizAdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuizAdminController(AppDbContext context)
        {
            _context = context;
        }

        #region Category Management

        /// <summary>
        /// Get all quiz categories (including inactive)
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<QuizCategoryDto>>> GetAllCategories()
        {
            try
            {
                var categories = await _context.QuizCategories
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new QuizCategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Icon = c.Icon,
                        DisplayOrder = c.DisplayOrder,
                        IsActive = c.IsActive,
                        TotalQuestions = c.Questions.Count
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
        /// Create new quiz category
        /// </summary>
        [HttpPost("categories")]
        public async Task<ActionResult<QuizCategoryDto>> CreateCategory([FromBody] CreateQuizCategoryDto request)
        {
            try
            {
                var category = new QuizCategory
                {
                    Name = request.Name,
                    Description = request.Description,
                    Icon = request.Icon,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = request.IsActive
                };

                _context.QuizCategories.Add(category);
                await _context.SaveChangesAsync();

                var response = new QuizCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Icon = category.Icon,
                    DisplayOrder = category.DisplayOrder,
                    IsActive = category.IsActive,
                    TotalQuestions = 0
                };

                return CreatedAtAction(nameof(GetAllCategories), new { id = category.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Update quiz category
        /// </summary>
        [HttpPut("categories/{id}")]
        public async Task<ActionResult> UpdateCategory(int id, [FromBody] CreateQuizCategoryDto request)
        {
            try
            {
                var category = await _context.QuizCategories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                category.Name = request.Name;
                category.Description = request.Description;
                category.Icon = request.Icon;
                category.DisplayOrder = request.DisplayOrder;
                category.IsActive = request.IsActive;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Categoría actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete quiz category (only if no questions exist)
        /// </summary>
        [HttpDelete("categories/{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.QuizCategories
                    .Include(c => c.Questions)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                if (category.Questions.Any())
                {
                    return BadRequest(new { message = "No se puede eliminar una categoría con preguntas. Elimine las preguntas primero." });
                }

                _context.QuizCategories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Categoría eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar categoría", error = ex.Message });
            }
        }

        #endregion

        #region Question Management

        /// <summary>
        /// Get all questions with filters
        /// </summary>
        [HttpGet("questions")]
        public async Task<ActionResult<List<QuizQuestionDto>>> GetAllQuestions(
            [FromQuery] int? categoryId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int? difficulty = null)
        {
            try
            {
                var query = _context.QuizQuestions
                    .Include(q => q.Category)
                    .Include(q => q.Answers)
                    .AsQueryable();

                if (categoryId.HasValue)
                {
                    query = query.Where(q => q.CategoryId == categoryId.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(q => q.IsActive == isActive.Value);
                }

                if (difficulty.HasValue)
                {
                    query = query.Where(q => q.Difficulty == difficulty.Value);
                }

                var questions = await query
                    .OrderBy(q => q.CategoryId)
                    .ThenBy(q => q.Id)
                    .Select(q => new QuizQuestionDto
                    {
                        Id = q.Id,
                        CategoryId = q.CategoryId,
                        CategoryName = q.Category!.Name,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Difficulty = q.Difficulty,
                        Points = q.Points,
                        ExplanationText = q.ExplanationText,
                        IsActive = q.IsActive,
                        Answers = q.Answers
                            .OrderBy(a => a.DisplayOrder)
                            .Select(a => new QuizAnswerDto
                            {
                                Id = a.Id,
                                QuestionId = a.QuestionId,
                                AnswerText = a.AnswerText,
                                IsCorrect = a.IsCorrect,
                                DisplayOrder = a.DisplayOrder
                            }).ToList()
                    })
                    .ToListAsync();

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener preguntas", error = ex.Message });
            }
        }

        /// <summary>
        /// Get single question by ID
        /// </summary>
        [HttpGet("questions/{id}")]
        public async Task<ActionResult<QuizQuestionDto>> GetQuestion(int id)
        {
            try
            {
                var question = await _context.QuizQuestions
                    .Include(q => q.Category)
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (question == null)
                {
                    return NotFound(new { message = "Pregunta no encontrada" });
                }

                var response = new QuizQuestionDto
                {
                    Id = question.Id,
                    CategoryId = question.CategoryId,
                    CategoryName = question.Category?.Name,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType,
                    Difficulty = question.Difficulty,
                    Points = question.Points,
                    ExplanationText = question.ExplanationText,
                    IsActive = question.IsActive,
                    Answers = question.Answers
                        .OrderBy(a => a.DisplayOrder)
                        .Select(a => new QuizAnswerDto
                        {
                            Id = a.Id,
                            QuestionId = a.QuestionId,
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect,
                            DisplayOrder = a.DisplayOrder
                        }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener pregunta", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new question with answers
        /// </summary>
        [HttpPost("questions")]
        public async Task<ActionResult<QuizQuestionDto>> CreateQuestion([FromBody] CreateQuizQuestionDto request)
        {
            try
            {
                // Validate category exists
                var categoryExists = await _context.QuizCategories.AnyAsync(c => c.Id == request.CategoryId);
                if (!categoryExists)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                // Validate answers
                if (request.Answers == null || !request.Answers.Any())
                {
                    return BadRequest(new { message = "Debe proporcionar al menos una respuesta" });
                }

                var correctAnswersCount = request.Answers.Count(a => a.IsCorrect);
                if (correctAnswersCount != 1)
                {
                    return BadRequest(new { message = "Debe haber exactamente una respuesta correcta" });
                }

                // Create question
                var question = new QuizQuestion
                {
                    CategoryId = request.CategoryId,
                    QuestionText = request.QuestionText,
                    QuestionType = request.QuestionType,
                    Difficulty = request.Difficulty,
                    Points = request.Points,
                    ExplanationText = request.ExplanationText,
                    IsActive = request.IsActive
                };

                _context.QuizQuestions.Add(question);
                await _context.SaveChangesAsync();

                // Create answers
                var answers = new List<QuizAnswer>();
                foreach (var answerDto in request.Answers)
                {
                    var answer = new QuizAnswer
                    {
                        QuestionId = question.Id,
                        AnswerText = answerDto.AnswerText,
                        IsCorrect = answerDto.IsCorrect,
                        DisplayOrder = answerDto.DisplayOrder
                    };
                    answers.Add(answer);
                }

                _context.QuizAnswers.AddRange(answers);
                await _context.SaveChangesAsync();

                // Load category for response
                await _context.Entry(question).Reference(q => q.Category).LoadAsync();

                var response = new QuizQuestionDto
                {
                    Id = question.Id,
                    CategoryId = question.CategoryId,
                    CategoryName = question.Category?.Name,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType,
                    Difficulty = question.Difficulty,
                    Points = question.Points,
                    ExplanationText = question.ExplanationText,
                    IsActive = question.IsActive,
                    Answers = answers.Select(a => new QuizAnswerDto
                    {
                        Id = a.Id,
                        QuestionId = a.QuestionId,
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect,
                        DisplayOrder = a.DisplayOrder
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear pregunta", error = ex.Message });
            }
        }

        /// <summary>
        /// Update question and answers
        /// </summary>
        [HttpPut("questions/{id}")]
        public async Task<ActionResult> UpdateQuestion(int id, [FromBody] CreateQuizQuestionDto request)
        {
            try
            {
                var question = await _context.QuizQuestions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (question == null)
                {
                    return NotFound(new { message = "Pregunta no encontrada" });
                }

                // Validate category exists
                var categoryExists = await _context.QuizCategories.AnyAsync(c => c.Id == request.CategoryId);
                if (!categoryExists)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                // Validate answers
                if (request.Answers == null || !request.Answers.Any())
                {
                    return BadRequest(new { message = "Debe proporcionar al menos una respuesta" });
                }

                var correctAnswersCount = request.Answers.Count(a => a.IsCorrect);
                if (correctAnswersCount != 1)
                {
                    return BadRequest(new { message = "Debe haber exactamente una respuesta correcta" });
                }

                // Update question
                question.CategoryId = request.CategoryId;
                question.QuestionText = request.QuestionText;
                question.QuestionType = request.QuestionType;
                question.Difficulty = request.Difficulty;
                question.Points = request.Points;
                question.ExplanationText = request.ExplanationText;
                question.IsActive = request.IsActive;

                // Remove old answers
                _context.QuizAnswers.RemoveRange(question.Answers);

                // Add new answers
                var newAnswers = new List<QuizAnswer>();
                foreach (var answerDto in request.Answers)
                {
                    var answer = new QuizAnswer
                    {
                        QuestionId = question.Id,
                        AnswerText = answerDto.AnswerText,
                        IsCorrect = answerDto.IsCorrect,
                        DisplayOrder = answerDto.DisplayOrder
                    };
                    newAnswers.Add(answer);
                }

                _context.QuizAnswers.AddRange(newAnswers);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Pregunta actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar pregunta", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete question (only if not used in attempts)
        /// </summary>
        [HttpDelete("questions/{id}")]
        public async Task<ActionResult> DeleteQuestion(int id)
        {
            try
            {
                var question = await _context.QuizQuestions
                    .Include(q => q.UserAnswers)
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (question == null)
                {
                    return NotFound(new { message = "Pregunta no encontrada" });
                }

                if (question.UserAnswers.Any())
                {
                    return BadRequest(new { message = "No se puede eliminar una pregunta que ya ha sido respondida. Puede desactivarla en su lugar." });
                }

                // Remove answers first
                _context.QuizAnswers.RemoveRange(question.Answers);
                _context.QuizQuestions.Remove(question);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Pregunta eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar pregunta", error = ex.Message });
            }
        }

        /// <summary>
        /// Toggle question active status
        /// </summary>
        [HttpPatch("questions/{id}/toggle-active")]
        public async Task<ActionResult> ToggleQuestionActive(int id)
        {
            try
            {
                var question = await _context.QuizQuestions.FindAsync(id);
                if (question == null)
                {
                    return NotFound(new { message = "Pregunta no encontrada" });
                }

                question.IsActive = !question.IsActive;
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Pregunta {(question.IsActive ? "activada" : "desactivada")} exitosamente", isActive = question.IsActive });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cambiar estado de pregunta", error = ex.Message });
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Bulk import questions from JSON
        /// </summary>
        [HttpPost("questions/bulk-import")]
        public async Task<ActionResult> BulkImportQuestions([FromBody] List<CreateQuizQuestionDto> questions)
        {
            try
            {
                if (questions == null || !questions.Any())
                {
                    return BadRequest(new { message = "No se proporcionaron preguntas para importar" });
                }

                var importedCount = 0;
                var errors = new List<string>();

                foreach (var questionDto in questions)
                {
                    try
                    {
                        // Validate category
                        var categoryExists = await _context.QuizCategories.AnyAsync(c => c.Id == questionDto.CategoryId);
                        if (!categoryExists)
                        {
                            errors.Add($"Categoría {questionDto.CategoryId} no encontrada para pregunta: {questionDto.QuestionText}");
                            continue;
                        }

                        // Validate answers
                        if (questionDto.Answers == null || !questionDto.Answers.Any())
                        {
                            errors.Add($"Sin respuestas para pregunta: {questionDto.QuestionText}");
                            continue;
                        }

                        var correctCount = questionDto.Answers.Count(a => a.IsCorrect);
                        if (correctCount != 1)
                        {
                            errors.Add($"Debe haber exactamente una respuesta correcta para: {questionDto.QuestionText}");
                            continue;
                        }

                        // Create question
                        var question = new QuizQuestion
                        {
                            CategoryId = questionDto.CategoryId,
                            QuestionText = questionDto.QuestionText,
                            QuestionType = questionDto.QuestionType,
                            Difficulty = questionDto.Difficulty,
                            Points = questionDto.Points,
                            ExplanationText = questionDto.ExplanationText,
                            IsActive = questionDto.IsActive
                        };

                        _context.QuizQuestions.Add(question);
                        await _context.SaveChangesAsync();

                        // Create answers
                        var answers = questionDto.Answers.Select(a => new QuizAnswer
                        {
                            QuestionId = question.Id,
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect,
                            DisplayOrder = a.DisplayOrder
                        }).ToList();

                        _context.QuizAnswers.AddRange(answers);
                        await _context.SaveChangesAsync();

                        importedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error al importar pregunta '{questionDto.QuestionText}': {ex.Message}");
                    }
                }

                return Ok(new
                {
                    message = $"Importación completada: {importedCount} preguntas importadas",
                    importedCount,
                    errorCount = errors.Count,
                    errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en importación masiva", error = ex.Message });
            }
        }

        /// <summary>
        /// Export all questions to JSON format
        /// </summary>
        [HttpGet("questions/export")]
        public async Task<ActionResult> ExportQuestions([FromQuery] int? categoryId = null)
        {
            try
            {
                var query = _context.QuizQuestions
                    .Include(q => q.Answers)
                    .AsQueryable();

                if (categoryId.HasValue)
                {
                    query = query.Where(q => q.CategoryId == categoryId.Value);
                }

                var questions = await query
                    .Select(q => new CreateQuizQuestionDto
                    {
                        CategoryId = q.CategoryId,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Difficulty = q.Difficulty,
                        Points = q.Points,
                        ExplanationText = q.ExplanationText,
                        IsActive = q.IsActive,
                        Answers = q.Answers.Select(a => new CreateQuizAnswerDto
                        {
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect,
                            DisplayOrder = a.DisplayOrder
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al exportar preguntas", error = ex.Message });
            }
        }

        #endregion
    }
}
