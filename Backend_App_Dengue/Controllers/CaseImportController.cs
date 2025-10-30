using Backend_App_Dengue.Attributes;
using Backend_App_Dengue.Data.Enums;
using Backend_App_Dengue.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend_App_Dengue.Controllers
{
    /// <summary>
    /// Controlador para importación masiva de casos epidemiológicos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación JWT
    public class CaseImportController : ControllerBase
    {
        private readonly CaseImportService _importService;
        private readonly ILogger<CaseImportController> _logger;

        public CaseImportController(CaseImportService importService, ILogger<CaseImportController> logger)
        {
            _importService = importService;
            _logger = logger;
        }

        /// <summary>
        /// Importa casos desde un archivo CSV
        /// Solo personal médico y administradores pueden importar
        /// </summary>
        /// <param name="file">Archivo CSV con los casos</param>
        /// <returns>Resultado de la importación con estadísticas</returns>
        [HttpPost("import-csv")]
        [RequirePermission(PermissionCode.CASE_IMPORT_CSV)]
        [ProducesResponseType(typeof(Model.Dto.CaseImportResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No se proporcionó ningún archivo" });
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "El archivo debe ser CSV (.csv)" });
            }

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                using var stream = file.OpenReadStream();
                var result = await _importService.ImportFromCsvAsync(stream, userId.Value);

                _logger.LogInformation($"Usuario {userId} importó {result.SuccessfulImports} casos desde CSV");

                return Ok(new
                {
                    message = "Importación completada",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la importación CSV");
                return StatusCode(500, new
                {
                    message = "Error al procesar el archivo CSV",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Importa casos desde un archivo Excel
        /// Solo personal médico y administradores pueden importar
        /// </summary>
        /// <param name="file">Archivo Excel (.xlsx) con los casos</param>
        /// <returns>Resultado de la importación con estadísticas</returns>
        [HttpPost("import-excel")]
        [RequirePermission(PermissionCode.CASE_IMPORT_CSV)]
        [ProducesResponseType(typeof(Model.Dto.CaseImportResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No se proporcionó ningún archivo" });
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "El archivo debe ser Excel (.xlsx)" });
            }

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                using var stream = file.OpenReadStream();
                var result = await _importService.ImportFromExcelAsync(stream, userId.Value);

                _logger.LogInformation($"Usuario {userId} importó {result.SuccessfulImports} casos desde Excel");

                return Ok(new
                {
                    message = "Importación completada",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la importación Excel");
                return StatusCode(500, new
                {
                    message = "Error al procesar el archivo Excel",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Descarga plantilla CSV para importación
        /// </summary>
        [HttpGet("download-template-csv")]
        [RequirePermission(PermissionCode.CASE_IMPORT_CSV)]
        public IActionResult DownloadCsvTemplate()
        {
            var csvContent = "año_,edad_,clasificacion del caso,sNA,bar_ver_,latitud (ISO),longitud (ISO),exo_,COMU\n" +
                           "2025,45,Dengue Clásico,,,4.5389,-75.6706,,\n" +
                           "2025,32,Dengue Hemorrágico,,,4.5400,-75.6800,,";

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
            return File(bytes, "text/csv", "plantilla_casos_dengue.csv");
        }

        /// <summary>
        /// Obtiene el ID del usuario actual desde el token JWT
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub")
                           ?? User.FindFirst("userId");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }
    }
}
