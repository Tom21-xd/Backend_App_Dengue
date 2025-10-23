using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Backend_App_Dengue.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CertificatePdfService _pdfService;
        private readonly ConexionMongo _conexionMongo;
        private const decimal PASSING_SCORE = 80.0m;

        public CertificateController(
            AppDbContext context,
            CertificatePdfService pdfService,
            ConexionMongo conexionMongo)
        {
            _context = context;
            _pdfService = pdfService;
            _conexionMongo = conexionMongo;
        }

        /// <summary>
        /// Generate certificate for a passed quiz attempt
        /// IMPORTANTE: Un usuario solo puede tener UN certificado. Si ya tiene uno, se revoca el anterior.
        /// </summary>
        [HttpPost("generate")]
        public async Task<ActionResult<CertificateDto>> GenerateCertificate([FromBody] GenerateCertificateDto request)
        {
            try
            {
                // Validate attempt exists and is completed
                var attempt = await _context.QuizAttempts
                    .Include(a => a.User)
                    .Include(a => a.Certificate)
                    .FirstOrDefaultAsync(a => a.Id == request.AttemptId);

                if (attempt == null)
                {
                    return NotFound(new { message = "Intento no encontrado" });
                }

                if (attempt.Status != "Completed")
                {
                    return BadRequest(new { message = "El quiz debe estar completado para generar certificado" });
                }

                if (attempt.Score < PASSING_SCORE)
                {
                    return BadRequest(new { message = $"La puntuación debe ser al menos {PASSING_SCORE}% para generar certificado" });
                }

                // IMPORTANTE: Check if user already has a certificate (one certificate per user policy)
                var existingUserCertificate = await _context.Certificates
                    .Where(c => c.UserId == attempt.UserId && c.Status == "Active")
                    .FirstOrDefaultAsync();

                if (existingUserCertificate != null)
                {
                    // If attempting to generate for same attempt, return existing
                    if (existingUserCertificate.AttemptId == request.AttemptId)
                    {
                        return Ok(new CertificateDto
                        {
                            Id = existingUserCertificate.Id,
                            VerificationCode = existingUserCertificate.VerificationCode,
                            IssuedAt = existingUserCertificate.IssuedAt,
                            Score = existingUserCertificate.Score,
                            UserName = attempt.User?.Name ?? "",
                            UserEmail = attempt.User?.Email ?? "",
                            PdfUrl = existingUserCertificate.PdfUrl,
                            Status = existingUserCertificate.Status
                        });
                    }

                    // If user has better score, revoke old certificate and generate new one
                    if (attempt.Score > existingUserCertificate.Score)
                    {
                        existingUserCertificate.Status = "Revoked";
                        // Delete old PDF from MongoDB if exists
                        if (!string.IsNullOrEmpty(existingUserCertificate.PdfUrl))
                        {
                            try
                            {
                                _conexionMongo.DeletePdf(existingUserCertificate.PdfUrl);
                            }
                            catch { /* Ignore if file doesn't exist */ }
                        }
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            message = "Ya tienes un certificado activo con mejor o igual puntuación",
                            existingCertificateId = existingUserCertificate.Id,
                            existingScore = existingUserCertificate.Score,
                            newScore = attempt.Score
                        });
                    }
                }

                // Generate unique verification code
                var verificationCode = GenerateVerificationCode(attempt.UserId, attempt.Id);

                // Generate PDF
                var certificateData = new CertificateData
                {
                    UserName = attempt.User?.Name ?? "",
                    UserEmail = attempt.User?.Email ?? "",
                    Score = attempt.Score,
                    TotalQuestions = attempt.TotalQuestions,
                    CorrectAnswers = attempt.CorrectAnswers,
                    IssuedAt = DateTime.UtcNow,
                    VerificationCode = verificationCode
                };

                var pdfBytes = _pdfService.GenerateCertificatePdf(certificateData);

                // Create certificate record first to get the ID
                var certificate = new Certificate
                {
                    UserId = attempt.UserId,
                    AttemptId = attempt.Id,
                    VerificationCode = verificationCode,
                    IssuedAt = DateTime.UtcNow,
                    Score = attempt.Score,
                    Status = "Active"
                };

                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();

                // Upload PDF to MongoDB
                var fileName = $"certificado_{attempt.UserId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                var pdfModel = new CertificatePdfModel
                {
                    PdfData = pdfBytes,
                    FileName = fileName,
                    CertificateId = certificate.Id
                };
                var pdfId = _conexionMongo.UploadPdf(pdfModel);

                // Update certificate with PDF ID
                certificate.PdfUrl = pdfId;
                await _context.SaveChangesAsync();

                var response = new CertificateDto
                {
                    Id = certificate.Id,
                    VerificationCode = certificate.VerificationCode,
                    IssuedAt = certificate.IssuedAt,
                    Score = certificate.Score,
                    UserName = certificateData.UserName,
                    UserEmail = certificateData.UserEmail,
                    PdfUrl = certificate.PdfUrl,
                    Status = certificate.Status
                };

                return CreatedAtAction(nameof(GetCertificate), new { id = certificate.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar certificado", error = ex.Message });
            }
        }

        /// <summary>
        /// Get certificate by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CertificateDto>> GetCertificate(int id)
        {
            try
            {
                var certificate = await _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Attempt)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (certificate == null)
                {
                    return NotFound(new { message = "Certificado no encontrado" });
                }

                var response = new CertificateDto
                {
                    Id = certificate.Id,
                    VerificationCode = certificate.VerificationCode,
                    IssuedAt = certificate.IssuedAt,
                    Score = certificate.Score,
                    UserName = certificate.User?.Name ?? "",
                    UserEmail = certificate.User?.Email ?? "",
                    PdfUrl = certificate.PdfUrl,
                    Status = certificate.Status
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener certificado", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user's certificates
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<CertificateDto>>> GetUserCertificates(int userId)
        {
            try
            {
                var certificates = await _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Attempt)
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.IssuedAt)
                    .Select(c => new CertificateDto
                    {
                        Id = c.Id,
                        VerificationCode = c.VerificationCode,
                        IssuedAt = c.IssuedAt,
                        Score = c.Score,
                        UserName = c.User!.Name,
                        UserEmail = c.User.Email,
                        PdfUrl = c.PdfUrl,
                        Status = c.Status
                    })
                    .ToListAsync();

                return Ok(certificates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener certificados del usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Verify certificate by verification code
        /// </summary>
        [HttpGet("verify/{verificationCode}")]
        public async Task<ActionResult<CertificateVerificationDto>> VerifyCertificate(string verificationCode)
        {
            try
            {
                var certificate = await _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Attempt)
                    .FirstOrDefaultAsync(c => c.VerificationCode == verificationCode);

                if (certificate == null)
                {
                    return Ok(new CertificateVerificationDto
                    {
                        IsValid = false,
                        Message = "Código de verificación no válido"
                    });
                }

                if (certificate.Status == "Revoked")
                {
                    return Ok(new CertificateVerificationDto
                    {
                        IsValid = false,
                        Message = "Este certificado ha sido revocado",
                        CertificateId = certificate.Id,
                        IssuedAt = certificate.IssuedAt,
                        RevokedDate = DateTime.UtcNow
                    });
                }

                var response = new CertificateVerificationDto
                {
                    IsValid = true,
                    Message = "Certificado válido",
                    CertificateId = certificate.Id,
                    UserName = certificate.User?.Name ?? "",
                    UserEmail = certificate.User?.Email ?? "",
                    IssuedAt = certificate.IssuedAt,
                    Score = certificate.Score,
                    AttemptId = certificate.AttemptId,
                    TotalQuestions = certificate.Attempt?.TotalQuestions ?? 0,
                    CorrectAnswers = certificate.Attempt?.CorrectAnswers ?? 0
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al verificar certificado", error = ex.Message });
            }
        }

        /// <summary>
        /// Revoke a certificate (Admin only)
        /// </summary>
        [HttpPatch("{id}/revoke")]
        public async Task<ActionResult> RevokeCertificate(int id, [FromBody] RevokeCertificateDto request)
        {
            try
            {
                var certificate = await _context.Certificates.FindAsync(id);
                if (certificate == null)
                {
                    return NotFound(new { message = "Certificado no encontrado" });
                }

                if (certificate.Status == "Revoked")
                {
                    return BadRequest(new { message = "El certificado ya está revocado" });
                }

                certificate.Status = "Revoked";
                await _context.SaveChangesAsync();

                return Ok(new { message = "Certificado revocado exitosamente", reason = request.Reason });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al revocar certificado", error = ex.Message });
            }
        }

        /// <summary>
        /// Download certificate PDF
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadCertificate(int id)
        {
            try
            {
                var certificate = await _context.Certificates
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (certificate == null)
                {
                    return NotFound(new { message = "Certificado no encontrado" });
                }

                if (string.IsNullOrEmpty(certificate.PdfUrl))
                {
                    return NotFound(new { message = "PDF no disponible" });
                }

                if (certificate.Status == "Revoked")
                {
                    return BadRequest(new { message = "Este certificado ha sido revocado" });
                }

                // Download PDF from MongoDB
                var pdfModel = _conexionMongo.GetPdf(certificate.PdfUrl);
                if (pdfModel == null || pdfModel.PdfData == null || pdfModel.PdfData.Length == 0)
                {
                    return NotFound(new { message = "Archivo PDF no encontrado" });
                }

                var fileName = $"Certificado_Dengue_{certificate.User?.Name?.Replace(" ", "_")}_{certificate.IssuedAt:yyyyMMdd}.pdf";

                return File(pdfModel.PdfData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al descargar certificado", error = ex.Message });
            }
        }

        /// <summary>
        /// Update certificate PDF URL (after PDF generation) - Deprecated, PDF is now auto-generated
        /// </summary>
        [HttpPatch("{id}/pdf-url")]
        [Obsolete("PDF is now automatically generated. This endpoint is deprecated.")]
        public async Task<ActionResult> UpdateCertificatePdfUrl(int id, [FromBody] UpdatePdfUrlDto request)
        {
            try
            {
                var certificate = await _context.Certificates.FindAsync(id);
                if (certificate == null)
                {
                    return NotFound(new { message = "Certificado no encontrado" });
                }

                certificate.PdfUrl = request.PdfUrl;
                await _context.SaveChangesAsync();

                return Ok(new { message = "URL del PDF actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar URL del PDF", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all certificates (Admin only)
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<List<CertificateDto>>> GetAllCertificates(
            [FromQuery] string? status = null,
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Attempt)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(c => c.Status == status);
                }

                if (userId.HasValue)
                {
                    query = query.Where(c => c.UserId == userId.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(c => c.IssuedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(c => c.IssuedAt <= endDate.Value);
                }

                var certificates = await query
                    .OrderByDescending(c => c.IssuedAt)
                    .Select(c => new CertificateDto
                    {
                        Id = c.Id,
                        VerificationCode = c.VerificationCode,
                        IssuedAt = c.IssuedAt,
                        Score = c.Score,
                        UserName = c.User!.Name,
                        UserEmail = c.User.Email,
                        PdfUrl = c.PdfUrl,
                        Status = c.Status
                    })
                    .ToListAsync();

                return Ok(certificates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener certificados", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if user is eligible for certificate
        /// </summary>
        [HttpGet("eligible/{attemptId}")]
        public async Task<ActionResult<CertificateEligibilityDto>> CheckEligibility(int attemptId)
        {
            try
            {
                var attempt = await _context.QuizAttempts
                    .Include(a => a.Certificate)
                    .FirstOrDefaultAsync(a => a.Id == attemptId);

                if (attempt == null)
                {
                    return NotFound(new { message = "Intento no encontrado" });
                }

                var isEligible = attempt.Status == "Completed" &&
                                 attempt.Score >= PASSING_SCORE &&
                                 attempt.Certificate == null;

                var response = new CertificateEligibilityDto
                {
                    IsEligible = isEligible,
                    AttemptId = attemptId,
                    Score = attempt.Score,
                    RequiredScore = PASSING_SCORE,
                    IsCompleted = attempt.Status == "Completed",
                    HasExistingCertificate = attempt.Certificate != null,
                    ExistingCertificateId = attempt.Certificate?.Id,
                    Message = isEligible
                        ? "Elegible para generar certificado"
                        : attempt.Status != "Completed"
                            ? "El quiz debe estar completado"
                            : attempt.Score < PASSING_SCORE
                                ? $"Se requiere una puntuación mínima de {PASSING_SCORE}%"
                                : "Ya existe un certificado para este intento"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al verificar elegibilidad", error = ex.Message });
            }
        }

        #region Helper Methods

        /// <summary>
        /// Generate unique verification code for certificate
        /// </summary>
        private string GenerateVerificationCode(int userId, int attemptId)
        {
            var timestamp = DateTime.UtcNow.Ticks;
            var input = $"{userId}-{attemptId}-{timestamp}-DENGUE-UCEVA";

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var hash = BitConverter.ToString(hashBytes).Replace("-", "");
                return $"CERT-{hash.Substring(0, 12)}-{timestamp.ToString().Substring(timestamp.ToString().Length - 6)}";
            }
        }

        #endregion
    }

    #region Certificate DTOs

    public class GenerateCertificateDto
    {
        public int AttemptId { get; set; }
    }

    public class CertificateVerificationDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? CertificateId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public DateTime? IssuedAt { get; set; }
        public decimal? Score { get; set; }
        public int? AttemptId { get; set; }
        public int? TotalQuestions { get; set; }
        public int? CorrectAnswers { get; set; }
        public DateTime? RevokedDate { get; set; }
    }

    public class RevokeCertificateDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdatePdfUrlDto
    {
        public string PdfUrl { get; set; } = string.Empty;
    }

    public class CertificateEligibilityDto
    {
        public bool IsEligible { get; set; }
        public int AttemptId { get; set; }
        public decimal Score { get; set; }
        public decimal RequiredScore { get; set; }
        public bool IsCompleted { get; set; }
        public bool HasExistingCertificate { get; set; }
        public int? ExistingCertificateId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
