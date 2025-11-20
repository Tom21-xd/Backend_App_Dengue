using Backend_App_Dengue.Attributes;
using Backend_App_Dengue.Data.Enums;
using Backend_App_Dengue.Model.Dto;
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
        /// <param name="columnMapping">Mapeo de columnas en formato JSON (opcional)</param>
        /// <returns>Resultado de la importación con estadísticas</returns>
        [HttpPost("import-csv")]
        [RequirePermission(PermissionCode.CASE_IMPORT_CSV)]
        [ProducesResponseType(typeof(Model.Dto.CaseImportResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ImportCsv(IFormFile file, [FromForm] string? columnMapping)
        {
            _logger.LogInformation("════════════════════════════════════════════════════");
            _logger.LogInformation("📨 REQUEST RECIBIDO: ImportCsv");
            _logger.LogInformation("════════════════════════════════════════════════════");

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("❌ No se proporcionó archivo");
                return BadRequest(new { message = "No se proporcionó ningún archivo" });
            }

            _logger.LogInformation($"📁 Archivo recibido: {file.FileName}");
            _logger.LogInformation($"📊 Tamaño: {file.Length} bytes");
            _logger.LogInformation($"🏷️  Content-Type: {file.ContentType}");

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning($"❌ Extensión inválida: {file.FileName}");
                return BadRequest(new { message = "El archivo debe ser CSV (.csv)" });
            }

            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation($"👤 Usuario autenticado: {userId}");

                if (userId == null)
                {
                    _logger.LogWarning("⚠️  Usuario no autenticado");
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation("🗺️  PROCESANDO MAPEO DE COLUMNAS");
                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation($"📦 columnMapping recibido (raw): {columnMapping ?? "NULL"}");
                _logger.LogInformation($"📏 Longitud: {columnMapping?.Length ?? 0} caracteres");

                // Parsear mapeo de columnas si existe
                Dictionary<string, string>? mapping = null;
                if (!string.IsNullOrWhiteSpace(columnMapping))
                {
                    try
                    {
                        mapping = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(columnMapping);
                        _logger.LogInformation($"✅ Mapeo parseado exitosamente: {mapping.Count} campos");
                        foreach (var kv in mapping)
                        {
                            _logger.LogInformation($"   {kv.Key} → {kv.Value}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"❌ Error al parsear mapeo de columnas: {ex.Message}");
                        _logger.LogWarning($"📝 JSON recibido: {columnMapping}");
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️  columnMapping está vacío o es NULL");
                }

                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation("🚀 LLAMANDO AL SERVICIO DE IMPORTACIÓN");
                _logger.LogInformation("────────────────────────────────────────────────────");

                using var stream = file.OpenReadStream();
                var result = await _importService.ImportFromCsvAsync(stream, userId.Value, mapping);

                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation("📤 PREPARANDO RESPUESTA");
                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation($"✅ Importación completada");
                _logger.LogInformation($"📊 Total: {result.TotalRows}");
                _logger.LogInformation($"✅ Exitosas: {result.SuccessfulImports}");
                _logger.LogInformation($"❌ Fallidas: {result.FailedImports}");
                _logger.LogInformation($"📦 ImportedCases count: {result.ImportedCases?.Count ?? 0}");
                _logger.LogInformation($"⏱️  Tiempo de procesamiento: {result.ProcessingTime.TotalSeconds:F2}s");

                var response = new
                {
                    message = "Importación completada",
                    data = result
                };

                var responseJson = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = false
                });
                _logger.LogInformation($"📨 Response JSON (primeros 500 chars): {responseJson.Substring(0, Math.Min(500, responseJson.Length))}...");

                _logger.LogInformation("════════════════════════════════════════════════════");
                _logger.LogInformation("✅ RESPUESTA ENVIADA AL CLIENTE");
                _logger.LogInformation("════════════════════════════════════════════════════");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("════════════════════════════════════════════════════");
                _logger.LogError("💥 ERROR EN CONTROLLER");
                _logger.LogError("════════════════════════════════════════════════════");
                _logger.LogError(ex, $"Tipo: {ex.GetType().Name}");
                _logger.LogError($"Mensaje: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");

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
        /// <param name="file">Archivo Excel (.xls o .xlsx) con los casos</param>
        /// <param name="columnMapping">Mapeo de columnas en formato JSON (opcional)</param>
        /// <returns>Resultado de la importación con estadísticas</returns>
        [HttpPost("import-excel")]
        [RequirePermission(PermissionCode.CASE_IMPORT_CSV)]
        [ProducesResponseType(typeof(Model.Dto.CaseImportResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ImportExcel(IFormFile file, [FromForm] string? columnMapping)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No se proporcionó ningún archivo" });
            }

            var isXls = file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase);
            var isXlsx = file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);

            if (!isXls && !isXlsx)
            {
                return BadRequest(new { message = "El archivo debe ser Excel (.xls o .xlsx)" });
            }

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                // Parsear mapeo de columnas si existe
                Dictionary<string, string>? mapping = null;
                if (!string.IsNullOrWhiteSpace(columnMapping))
                {
                    try
                    {
                        mapping = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(columnMapping);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error al parsear mapeo de columnas: {ex.Message}");
                    }
                }

                using var stream = file.OpenReadStream();
                var result = await _importService.ImportFromExcelAsync(stream, userId.Value, mapping);

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
                           "2025,45,Dengue sin signos de alarma,,,4.5389,-75.6706,,\n" +
                           "2025,32,Dengue con signos de alarma,,,4.5400,-75.6800,,\n" +
                           "2025,28,Dengue grave,,,4.109.694.509,-75.123.456.789,,";

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
            return File(bytes, "text/csv", "plantilla_casos_dengue.csv");
        }

        /// <summary>
        /// Descarga plantilla Excel para importación
        /// </summary>
        [HttpGet("download-template-excel")]
        [RequirePermission(PermissionCode.CASE_IMPORT_CSV)]
        public IActionResult DownloadExcelTemplate()
        {
            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Casos Dengue");

                // Crear encabezados
                worksheet.Cell(1, 1).Value = "año_";
                worksheet.Cell(1, 2).Value = "edad_";
                worksheet.Cell(1, 3).Value = "clasificacion del caso";
                worksheet.Cell(1, 4).Value = "sNA";
                worksheet.Cell(1, 5).Value = "bar_ver_";
                worksheet.Cell(1, 6).Value = "latitud (ISO)";
                worksheet.Cell(1, 7).Value = "longitud (ISO)";
                worksheet.Cell(1, 8).Value = "exo_";
                worksheet.Cell(1, 9).Value = "COMU";

                // Estilo de encabezados
                var headerRange = worksheet.Range(1, 1, 1, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

                // Agregar datos de ejemplo
                worksheet.Cell(2, 1).Value = 2025;
                worksheet.Cell(2, 2).Value = 45;
                worksheet.Cell(2, 3).Value = "Dengue sin signos de alarma";
                worksheet.Cell(2, 4).Value = "";
                worksheet.Cell(2, 5).Value = "Centro";
                worksheet.Cell(2, 6).Value = "4.5389";
                worksheet.Cell(2, 7).Value = "-75.6706";
                worksheet.Cell(2, 8).Value = "";
                worksheet.Cell(2, 9).Value = "";

                worksheet.Cell(3, 1).Value = 2025;
                worksheet.Cell(3, 2).Value = 32;
                worksheet.Cell(3, 3).Value = "Dengue con signos de alarma";
                worksheet.Cell(3, 4).Value = "";
                worksheet.Cell(3, 5).Value = "Norte";
                worksheet.Cell(3, 6).Value = "4.5400";
                worksheet.Cell(3, 7).Value = "-75.6800";
                worksheet.Cell(3, 8).Value = "";
                worksheet.Cell(3, 9).Value = "";

                worksheet.Cell(4, 1).Value = 2025;
                worksheet.Cell(4, 2).Value = 28;
                worksheet.Cell(4, 3).Value = "Dengue grave";
                worksheet.Cell(4, 4).Value = "";
                worksheet.Cell(4, 5).Value = "Sur";
                worksheet.Cell(4, 6).Value = "4.109.694.509";
                worksheet.Cell(4, 7).Value = "-75.123.456.789";
                worksheet.Cell(4, 8).Value = "";
                worksheet.Cell(4, 9).Value = "";

                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();

                using var stream = new System.IO.MemoryStream();
                workbook.SaveAs(stream);
                var bytes = stream.ToArray();

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "plantilla_casos_dengue.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar plantilla Excel");
                return StatusCode(500, new { message = "Error al generar la plantilla Excel", error = ex.Message });
            }
        }

        /// <summary>
        /// Valida columnas de archivo antes de importar
        /// Retorna columnas detectadas y mapeo sugerido
        /// </summary>
        [HttpPost("validate-columns")]
        [RequirePermission(PermissionCode.CASE_IMPORT_CSV)]
        public async Task<IActionResult> ValidateColumns(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No se proporcionó ningún archivo" });
            }

            var isExcel = file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                          file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase);
            var isCsv = file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

            if (!isExcel && !isCsv)
            {
                return BadRequest(new { message = "El archivo debe ser CSV o Excel" });
            }

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _importService.ValidateColumnsAsync(stream, isExcel);

                return Ok(new
                {
                    message = "Validación completada",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar columnas");
                return StatusCode(500, new { message = "Error al validar archivo", error = ex.Message });
            }
        }

        /// <summary>
        /// Importa casos con mapeo personalizado de columnas
        /// Soporta mapear múltiples columnas a un solo campo
        /// </summary>
        /// <param name="file">Archivo CSV o Excel</param>
        /// <param name="columnMapping">JSON con mapeo de columnas.
        /// Formato: { "mapping": { "ciudad": ["col1", "col2"], "edad": ["edad_"] }, "separator": " " }
        /// </param>
        [HttpPost("import-with-mapping")]
        [RequirePermission(PermissionCode.CASE_IMPORT_CSV)]
        public async Task<IActionResult> ImportWithMapping(
            IFormFile file,
            [FromForm] string columnMapping)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No se proporcionó ningún archivo" });
            }

            if (string.IsNullOrWhiteSpace(columnMapping))
            {
                return BadRequest(new { message = "Debe proporcionar el mapeo de columnas" });
            }

            var isExcel = file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                          file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase);
            var isCsv = file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

            if (!isExcel && !isCsv)
            {
                return BadRequest(new { message = "El archivo debe ser CSV o Excel" });
            }

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                // Intentar deserializar como ColumnMappingDto primero (nuevo formato)
                ColumnMappingDto? mappingDto = null;
                try
                {
                    mappingDto = System.Text.Json.JsonSerializer.Deserialize<ColumnMappingDto>(columnMapping);
                }
                catch
                {
                    // Si falla, intentar formato antiguo
                }

                using var stream = file.OpenReadStream();
                CaseImportResultDto result;

                if (mappingDto?.Mapping != null && mappingDto.Mapping.Count > 0)
                {
                    // Usar nuevo formato con múltiples columnas
                    result = await _importService.ImportWithCustomMappingAsync(stream, userId.Value, mappingDto, isExcel);
                }
                else
                {
                    // Fallback a formato antiguo (simple)
                    var mapping = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(columnMapping);
                    if (mapping == null)
                    {
                        return BadRequest(new { message = "Mapeo de columnas inválido" });
                    }
                    result = await _importService.ImportWithCustomMappingAsync(stream, userId.Value, mapping, isExcel);
                }

                _logger.LogInformation($"Usuario {userId} importó {result.SuccessfulImports} casos");

                return Ok(new
                {
                    message = "Importación completada",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la importación");
                return StatusCode(500, new
                {
                    message = "Error al importar archivo",
                    error = ex.Message
                });
            }
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
