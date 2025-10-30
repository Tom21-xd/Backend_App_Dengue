using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Model.Dto;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace Backend_App_Dengue.Services
{
    /// <summary>
    /// Servicio para importación masiva de casos desde archivos CSV/Excel
    /// </summary>
    public class CaseImportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CaseImportService> _logger;

        public CaseImportService(AppDbContext context, ILogger<CaseImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Importa casos desde un archivo CSV
        /// </summary>
        public async Task<CaseImportResultDto> ImportFromCsvAsync(Stream fileStream, int importedByUserId)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new CaseImportResultDto
            {
                ImportedAt = DateTime.Now,
                ImportedByUserId = importedByUserId
            };

            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null
                };

                using var reader = new StreamReader(fileStream);
                using var csv = new CsvReader(reader, config);

                var records = csv.GetRecords<CaseImportDto>().ToList();
                result.TotalRows = records.Count;

                int rowNumber = 1; // Inicia en 1 (header es 0)
                foreach (var record in records)
                {
                    rowNumber++;
                    try
                    {
                        var caseEntity = await MapToCaseEntityAsync(record, importedByUserId);
                        _context.Cases.Add(caseEntity);
                        result.SuccessfulImports++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedImports++;
                        result.Errors.Add(new CaseImportErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = ex.Message,
                            RowData = new Dictionary<string, string?>
                            {
                                { "año", record.Year },
                                { "edad", record.Age },
                                { "clasificacion", record.Classification },
                                { "barrio", record.Neighborhood },
                                { "latitud", record.Latitude },
                                { "longitud", record.Longitude }
                            }
                        });
                        _logger.LogWarning($"Error al procesar fila {rowNumber}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation($"Importación completada: {result.SuccessfulImports}/{result.TotalRows} casos importados exitosamente");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante la importación CSV");
                throw;
            }
        }

        /// <summary>
        /// Importa casos desde un archivo Excel (.xlsx)
        /// </summary>
        public async Task<CaseImportResultDto> ImportFromExcelAsync(Stream fileStream, int importedByUserId)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new CaseImportResultDto
            {
                ImportedAt = DateTime.Now,
                ImportedByUserId = importedByUserId
            };

            try
            {
                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheet(1); // Primera hoja
                var rows = worksheet.RowsUsed().Skip(1); // Saltar el header

                result.TotalRows = rows.Count();

                int rowNumber = 1;
                foreach (var row in rows)
                {
                    rowNumber++;
                    try
                    {
                        var record = new CaseImportDto
                        {
                            Year = row.Cell(1).GetString(),
                            Age = row.Cell(2).GetString(),
                            Classification = row.Cell(3).GetString(),
                            SNA = row.Cell(4).GetString(),
                            Neighborhood = row.Cell(5).GetString(),
                            Latitude = row.Cell(6).GetString(),
                            Longitude = row.Cell(7).GetString(),
                            Exo = row.Cell(8).GetString(),
                            Community = row.Cell(9).GetString()
                        };

                        var caseEntity = await MapToCaseEntityAsync(record, importedByUserId);
                        _context.Cases.Add(caseEntity);
                        result.SuccessfulImports++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedImports++;
                        result.Errors.Add(new CaseImportErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = ex.Message,
                            RowData = new Dictionary<string, string?>
                            {
                                { "Row", rowNumber.ToString() }
                            }
                        });
                        _logger.LogWarning($"Error al procesar fila {rowNumber}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation($"Importación Excel completada: {result.SuccessfulImports}/{result.TotalRows} casos importados exitosamente");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante la importación Excel");
                throw;
            }
        }

        /// <summary>
        /// Mapea un registro CSV/Excel a una entidad Case
        /// </summary>
        private async Task<Case> MapToCaseEntityAsync(CaseImportDto dto, int importedByUserId)
        {
            // Parsear año
            if (!int.TryParse(dto.Year, out int year))
            {
                throw new ArgumentException($"Año inválido: {dto.Year}");
            }

            // Parsear edad
            if (!int.TryParse(dto.Age, out int age))
            {
                throw new ArgumentException($"Edad inválida: {dto.Age}");
            }

            // Buscar tipo de dengue por clasificación (nombre)
            var dengueType = await _context.TypesOfDengue
                .Where(t => t.IsActive && t.Name.ToLower().Contains(dto.Classification!.ToLower()))
                .FirstOrDefaultAsync();

            if (dengueType == null)
            {
                throw new ArgumentException($"Clasificación de dengue no encontrada: {dto.Classification}");
            }

            // Parsear coordenadas
            decimal? latitude = null;
            decimal? longitude = null;

            if (!string.IsNullOrWhiteSpace(dto.Latitude) && decimal.TryParse(dto.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lat))
            {
                latitude = lat;
            }

            if (!string.IsNullOrWhiteSpace(dto.Longitude) && decimal.TryParse(dto.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lon))
            {
                longitude = lon;
            }

            // Obtener estado inicial (asumimos ID 1 = "Reportado")
            var initialStateId = 1;

            return new Case
            {
                Description = $"Caso importado - {dto.Classification}",
                Year = year,
                Age = age,
                TypeOfDengueId = dengueType.Id,
                Neighborhood = dto.Neighborhood,
                Latitude = latitude,
                Longitude = longitude,
                Address = dto.Neighborhood, // Usar barrio como dirección
                StateId = initialStateId,
                RegisteredByUserId = importedByUserId,
                CreatedAt = DateTime.Now,
                IsActive = true,
                // Campos opcionales que quedan null:
                // PatientId, HospitalId, MedicalStaffId, TemporaryName
            };
        }
    }
}
