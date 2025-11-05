using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Backend_App_Dengue.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Backend_App_Dengue.Controllers
{
    /// <summary>
    /// Controlador para gestión de certificados del sistema de quiz
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class CertificateController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CertificatePdfService _pdfService;
        private readonly ConexionMongo _conexionMongo;
        private readonly ILogger<CertificateController> _logger;
        private const decimal PASSING_SCORE = 80.0m;

        public CertificateController(
            AppDbContext context,
            CertificatePdfService pdfService,
            ConexionMongo conexionMongo,
            ILogger<CertificateController> logger)
        {
            _context = context;
            _pdfService = pdfService;
            _conexionMongo = conexionMongo;
            _logger = logger;
        }

        /// <summary>
        /// Genera un certificado PDF para el usuario autenticado basado en su mejor intento aprobado
        /// </summary>
        /// <returns>Certificado generado con código de verificación y URL del PDF</returns>
        /// <response code="200">Certificado generado o existente retornado exitosamente</response>
        /// <response code="400">No tiene intentos aprobados o puntuación insuficiente</response>
        /// <response code="401">No autenticado</response>
        /// <response code="500">Error al generar PDF o guardar en base de datos</response>
        /// <remarks>
        /// IMPORTANTE:
        /// - Requiere autenticación JWT (token Bearer)
        /// - Busca automáticamente el mejor intento aprobado (≥80%) del usuario
        /// - Un usuario solo puede tener UN certificado activo
        /// - Si ya tiene certificado, retorna el existente o lo reemplaza si tiene mejor puntuación
        /// </remarks>
        [Authorize]
        [HttpPost("generate")]
        [ProducesResponseType(typeof(CertificateDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CertificateDto>> GenerateCertificate()
        {
            try
            {
                _logger.LogInformation("=== CERTIFICATE GENERATION STARTED ===");

                // 1. Obtener userId del token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogError("No se pudo obtener el userId del token JWT");
                    return Unauthorized(new { message = "Token inválido o userId no encontrado" });
                }

                _logger.LogInformation($"Usuario autenticado - UserId: {userId}");

                // 2. Verificar que el usuario existe
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogError($"Usuario no encontrado - UserId: {userId}");
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                _logger.LogInformation($"Usuario encontrado: {user.Name} ({user.Email})");

                // 3. Buscar el mejor intento aprobado del usuario (≥80%, completado)
                var bestAttempt = await _context.QuizAttempts
                    .Where(a => a.UserId == userId && a.Status == "Completed" && a.Score >= PASSING_SCORE)
                    .OrderByDescending(a => a.Score)
                    .ThenByDescending(a => a.CompletedAt)
                    .FirstOrDefaultAsync();

                if (bestAttempt == null)
                {
                    _logger.LogWarning($"Usuario no tiene intentos aprobados - UserId: {userId}");
                    return BadRequest(new
                    {
                        message = $"No tienes ningún intento aprobado. Necesitas obtener al menos {PASSING_SCORE}% en el quiz para generar un certificado.",
                        requiredScore = PASSING_SCORE
                    });
                }

                _logger.LogInformation($"Mejor intento encontrado - AttemptId: {bestAttempt.Id}, Score: {bestAttempt.Score}%, Correct: {bestAttempt.CorrectAnswers}/{bestAttempt.TotalQuestions}");

                // 4. Verificar si ya tiene un certificado activo
                var existingCertificate = await _context.Certificates
                    .Where(c => c.UserId == userId && c.Status == "Active")
                    .FirstOrDefaultAsync();

                if (existingCertificate != null)
                {
                    _logger.LogInformation($"Usuario ya tiene certificado activo - CertificateId: {existingCertificate.Id}, Score: {existingCertificate.Score}%");

                    // Si el certificado es del mismo intento, retornarlo
                    if (existingCertificate.AttemptId == bestAttempt.Id)
                    {
                        _logger.LogInformation("Retornando certificado existente del mismo intento");
                        return Ok(new CertificateDto
                        {
                            Id = existingCertificate.Id,
                            VerificationCode = existingCertificate.VerificationCode,
                            IssuedAt = existingCertificate.IssuedAt,
                            Score = existingCertificate.Score,
                            UserName = user.Name,
                            UserEmail = user.Email,
                            PdfUrl = existingCertificate.PdfUrl,
                            Status = existingCertificate.Status
                        });
                    }

                    // Si el nuevo intento tiene mejor puntuación, revocar el anterior
                    if (bestAttempt.Score > existingCertificate.Score)
                    {
                        _logger.LogInformation($"Revocando certificado anterior (Score: {existingCertificate.Score}%) para generar nuevo (Score: {bestAttempt.Score}%)");
                        existingCertificate.Status = "Revoked";

                        // Eliminar PDF antiguo de MongoDB
                        if (!string.IsNullOrEmpty(existingCertificate.PdfUrl))
                        {
                            try
                            {
                                _conexionMongo.DeletePdf(existingCertificate.PdfUrl);
                                _logger.LogInformation($"PDF anterior eliminado: {existingCertificate.PdfUrl}");
                            }
                            catch (Exception delEx)
                            {
                                _logger.LogWarning($"No se pudo eliminar PDF anterior: {delEx.Message}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Certificado existente tiene igual o mejor puntuación");
                        return Ok(new CertificateDto
                        {
                            Id = existingCertificate.Id,
                            VerificationCode = existingCertificate.VerificationCode,
                            IssuedAt = existingCertificate.IssuedAt,
                            Score = existingCertificate.Score,
                            UserName = user.Name,
                            UserEmail = user.Email,
                            PdfUrl = existingCertificate.PdfUrl,
                            Status = existingCertificate.Status
                        });
                    }
                }

                // 5. Generar código de verificación único
                var verificationCode = GenerateVerificationCode(userId, bestAttempt.Id);
                _logger.LogInformation($"Código de verificación generado: {verificationCode}");

                // 6. Preparar datos del certificado
                var certificateData = new CertificateData
                {
                    UserName = user.Name,
                    UserEmail = user.Email,
                    Score = bestAttempt.Score,
                    TotalQuestions = bestAttempt.TotalQuestions,
                    CorrectAnswers = bestAttempt.CorrectAnswers,
                    IssuedAt = DateTime.UtcNow,
                    VerificationCode = verificationCode
                };

                // 7. Generar PDF
                _logger.LogInformation("Generando PDF del certificado...");
                byte[] pdfBytes;
                try
                {
                    pdfBytes = _pdfService.GenerateCertificatePdf(certificateData);
                    _logger.LogInformation($"PDF generado exitosamente - Tamaño: {pdfBytes.Length} bytes");
                }
                catch (Exception pdfEx)
                {
                    _logger.LogError(pdfEx, "Error al generar PDF del certificado");
                    return StatusCode(500, new
                    {
                        message = "Error al generar el PDF del certificado",
                        error = pdfEx.Message,
                        detail = "Posiblemente faltan archivos de recursos (logos) en el servidor"
                    });
                }

                // 8. Crear registro del certificado en MySQL
                var certificate = new Certificate
                {
                    UserId = userId,
                    AttemptId = bestAttempt.Id,
                    VerificationCode = verificationCode,
                    IssuedAt = DateTime.UtcNow,
                    Score = bestAttempt.Score,
                    Status = "Active"
                };

                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Certificado guardado en BD - CertificateId: {certificate.Id}");

                // 9. Subir PDF a MongoDB
                try
                {
                    var fileName = $"certificado_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                    var pdfModel = new CertificatePdfModel
                    {
                        PdfData = pdfBytes,
                        FileName = fileName,
                        CertificateId = certificate.Id
                    };
                    var pdfId = _conexionMongo.UploadPdf(pdfModel);

                    certificate.PdfUrl = pdfId;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"PDF subido a MongoDB - PdfId: {pdfId}");
                }
                catch (Exception mongoEx)
                {
                    _logger.LogError(mongoEx, "Error al subir PDF a MongoDB");
                    // Eliminar certificado de MySQL si falla MongoDB
                    _context.Certificates.Remove(certificate);
                    await _context.SaveChangesAsync();
                    return StatusCode(500, new
                    {
                        message = "Error al guardar el PDF del certificado",
                        error = mongoEx.Message
                    });
                }

                // 10. Enviar certificado por correo electrónico
                try
                {
                    _logger.LogInformation($"Enviando certificado por email a: {user.Email}");
                    ServiceGmail emailService = new ServiceGmail();
                    string htmlBody = EmailTemplates.CertificateTemplate(
                        user.Name,
                        user.Email,
                        certificate.Score,
                        certificate.VerificationCode,
                        certificate.IssuedAt
                    );

                    var pdfFileName = $"Certificado_Dengue_{user.Name.Replace(" ", "_")}_{certificate.IssuedAt:yyyyMMdd}.pdf";

                    await Task.Run(() => emailService.SendEmailWithPdfAttachment(
                        user.Email,
                        "🏆 ¡Tu Certificado de Dengue Track está listo!",
                        htmlBody,
                        pdfBytes,
                        pdfFileName
                    ));
                    _logger.LogInformation("Email enviado exitosamente");
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Error al enviar email, pero certificado fue generado exitosamente");
                    // No fallar si el email no se envía, el certificado ya está generado
                }

                // 11. Retornar respuesta exitosa
                var response = new CertificateDto
                {
                    Id = certificate.Id,
                    VerificationCode = certificate.VerificationCode,
                    IssuedAt = certificate.IssuedAt,
                    Score = certificate.Score,
                    UserName = user.Name,
                    UserEmail = user.Email,
                    PdfUrl = certificate.PdfUrl,
                    Status = certificate.Status
                };

                _logger.LogInformation("=== CERTIFICATE GENERATION COMPLETED SUCCESSFULLY ===");
                return CreatedAtAction(nameof(GetCertificate), new { id = certificate.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al generar certificado");
                return StatusCode(500, new
                {
                    message = "Error interno al generar certificado",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Obtiene un certificado por su ID
        /// </summary>
        /// <param name="id">ID del certificado</param>
        /// <returns>Datos del certificado solicitado</returns>
        /// <response code="200">Certificado encontrado y retornado exitosamente</response>
        /// <response code="404">Certificado no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CertificateDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
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
        /// Obtiene todos los certificados de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de certificados del usuario, ordenados por fecha de emisión descendente</returns>
        /// <response code="200">Lista de certificados retornada exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(List<CertificateDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<CertificateDto>>> GetUserCertificates(int userId)
        {
            try
            {
                // Verificar que el usuario existe primero
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                var certificates = await _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Attempt)
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.IssuedAt)
                    .ToListAsync();

                var result = certificates.Select(c => new CertificateDto
                {
                    Id = c.Id,
                    VerificationCode = c.VerificationCode,
                    IssuedAt = c.IssuedAt,
                    Score = c.Score,
                    UserName = c.User?.Name ?? "Usuario Desconocido",
                    UserEmail = c.User?.Email ?? "",
                    PdfUrl = c.PdfUrl,
                    Status = c.Status
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener certificados del usuario", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Verifica la validez de un certificado usando su código de verificación
        /// </summary>
        /// <param name="verificationCode">Código de verificación del certificado</param>
        /// <returns>Resultado de la verificación con datos del certificado si es válido</returns>
        /// <response code="200">Verificación completada (válida o inválida)</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Retorna información completa del certificado si es válido y activo.
        /// Si el certificado fue revocado, indica la fecha de revocación.
        /// </remarks>
        [HttpGet("verify/{verificationCode}")]
        [ProducesResponseType(typeof(CertificateVerificationDto), 200)]
        [ProducesResponseType(500)]
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
        /// Revoca un certificado (Solo administradores)
        /// </summary>
        /// <param name="id">ID del certificado a revocar</param>
        /// <param name="request">Razón de la revocación</param>
        /// <returns>Confirmación de revocación</returns>
        /// <response code="200">Certificado revocado exitosamente</response>
        /// <response code="400">El certificado ya está revocado</response>
        /// <response code="404">Certificado no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPatch("{id}/revoke")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
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
        /// Descarga el PDF de un certificado
        /// </summary>
        /// <param name="id">ID del certificado</param>
        /// <returns>Archivo PDF del certificado</returns>
        /// <response code="200">PDF retornado exitosamente</response>
        /// <response code="400">Certificado revocado</response>
        /// <response code="404">Certificado o PDF no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// No permite descargar certificados que han sido revocados.
        /// El PDF se obtiene desde MongoDB GridFS.
        /// </remarks>
        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
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
        /// Actualiza la URL del PDF de un certificado - DEPRECADO
        /// </summary>
        /// <param name="id">ID del certificado</param>
        /// <param name="request">Nueva URL del PDF</param>
        /// <returns>Confirmación de actualización</returns>
        /// <response code="200">URL actualizada exitosamente</response>
        /// <response code="404">Certificado no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// DEPRECADO: El PDF ahora se genera automáticamente al crear el certificado.
        /// Este endpoint se mantiene por compatibilidad pero no debería usarse.
        /// </remarks>
        [HttpPatch("{id}/pdf-url")]
        [Obsolete("PDF is now automatically generated. This endpoint is deprecated.")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
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
        /// Obtiene todos los certificados con filtros opcionales (Solo administradores)
        /// </summary>
        /// <param name="status">Estado del certificado (Active/Revoked)</param>
        /// <param name="userId">ID del usuario para filtrar</param>
        /// <param name="startDate">Fecha inicial de emisión</param>
        /// <param name="endDate">Fecha final de emisión</param>
        /// <returns>Lista de certificados que cumplen los filtros</returns>
        /// <response code="200">Lista de certificados retornada exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("all")]
        [ProducesResponseType(typeof(List<CertificateDto>), 200)]
        [ProducesResponseType(500)]
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
        /// Reenvía el certificado por correo electrónico con el PDF adjunto
        /// </summary>
        /// <param name="id">ID del certificado</param>
        /// <returns>Confirmación del reenvío</returns>
        /// <response code="200">Certificado reenviado exitosamente</response>
        /// <response code="400">Certificado revocado o sin PDF</response>
        /// <response code="404">Certificado no encontrado</response>
        /// <response code="500">Error al reenviar certificado</response>
        /// <remarks>
        /// Permite reenviar el certificado por email si el usuario lo perdió o necesita una copia adicional.
        /// El certificado debe estar activo y tener un PDF asociado.
        /// </remarks>
        [HttpPost("{id}/resend-email")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> ResendCertificateEmail(int id)
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

                if (certificate.User == null)
                {
                    return BadRequest(new { message = "El usuario asociado al certificado no fue encontrado" });
                }

                if (certificate.Status == "Revoked")
                {
                    return BadRequest(new { message = "No se puede reenviar un certificado revocado" });
                }

                if (string.IsNullOrEmpty(certificate.PdfUrl))
                {
                    return BadRequest(new { message = "El certificado no tiene PDF asociado" });
                }

                // Obtener el PDF desde MongoDB
                var pdfModel = _conexionMongo.GetPdf(certificate.PdfUrl);
                if (pdfModel == null || pdfModel.PdfData == null || pdfModel.PdfData.Length == 0)
                {
                    return BadRequest(new { message = "No se pudo obtener el archivo PDF" });
                }

                // Enviar email con el certificado
                try
                {
                    ServiceGmail emailService = new ServiceGmail();
                    string htmlBody = EmailTemplates.CertificateTemplate(
                        certificate.User.Name,
                        certificate.User.Email,
                        certificate.Score,
                        certificate.VerificationCode,
                        certificate.IssuedAt
                    );

                    var pdfFileName = $"Certificado_Dengue_{certificate.User.Name.Replace(" ", "_")}_{certificate.IssuedAt:yyyyMMdd}.pdf";

                    await Task.Run(() => emailService.SendEmailWithPdfAttachment(
                        certificate.User.Email,
                        "🏆 ¡Tu Certificado de Dengue Track está listo!",
                        htmlBody,
                        pdfModel.PdfData,
                        pdfFileName
                    ));

                    return Ok(new
                    {
                        message = "Certificado reenviado exitosamente",
                        email = certificate.User.Email,
                        certificateId = certificate.Id
                    });
                }
                catch (Exception emailEx)
                {
                    return StatusCode(500, new { message = "Error al enviar el correo", error = emailEx.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al reenviar certificado", error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si un intento de quiz es elegible para generar certificado
        /// </summary>
        /// <param name="attemptId">ID del intento de quiz</param>
        /// <returns>Información de elegibilidad con detalles</returns>
        /// <response code="200">Verificación completada exitosamente</response>
        /// <response code="404">Intento no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        /// <remarks>
        /// Un intento es elegible si:
        /// - Está completado (Status = "Completed")
        /// - Tiene puntuación >= 80%
        /// - No tiene un certificado generado previamente
        /// </remarks>
        [HttpGet("eligible/{attemptId}")]
        [ProducesResponseType(typeof(CertificateEligibilityDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
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
