using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model.Dto;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Backend_App_Dengue.Services
{
    /// <summary>
    /// Servicio para importación masiva de casos desde archivos CSV/Excel
    /// </summary>
    public class CaseImportService
    {
        private readonly AppDbContext _context;
        private readonly IRepository<TypeOfDengue> _dengueTypeRepository;
        private readonly ILogger<CaseImportService> _logger;

        public CaseImportService(
            AppDbContext context,
            IRepository<TypeOfDengue> dengueTypeRepository,
            ILogger<CaseImportService> logger)
        {
            _context = context;
            _dengueTypeRepository = dengueTypeRepository;
            _logger = logger;
        }

        /// <summary>
        /// Importa casos desde un archivo CSV
        /// </summary>
        public async Task<CaseImportResultDto> ImportFromCsvAsync(Stream fileStream, int importedByUserId, Dictionary<string, string>? columnMapping = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new CaseImportResultDto
            {
                ImportedAt = DateTime.Now,
                ImportedByUserId = importedByUserId
            };

            try
            {
                using var reader = new StreamReader(fileStream);

                // Leer primera línea para obtener los encabezados
                var headerLine = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(headerLine))
                {
                    throw new ArgumentException("El archivo CSV está vacío");
                }

                // Detectar delimitador
                var delimiter = headerLine.Count(c => c == ';') > headerLine.Count(c => c == ',') ? ";" : ",";
                var headers = headerLine.Split(delimiter).Select(h => h.Trim().Trim('"')).ToList();

                _logger.LogInformation($"Headers detectados: {string.Join(", ", headers)}");
                _logger.LogInformation($"Mapeo recibido: {(columnMapping != null ? string.Join(", ", columnMapping.Select(kv => $"{kv.Key}={kv.Value}")) : "ninguno")}");

                int rowNumber = 1;
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    rowNumber++;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        var values = line.Split(delimiter).Select(v => v.Trim().Trim('"')).ToArray();
                        var rowData = new Dictionary<string, string>();

                        // Crear diccionario de valores por columna
                        for (int i = 0; i < Math.Min(headers.Count, values.Length); i++)
                        {
                            rowData[headers[i]] = values[i];
                        }

                        var caseEntity = await MapRowToCaseEntityAsync(rowData, columnMapping, importedByUserId);
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
                            RowData = new Dictionary<string, string?> { { "Row", rowNumber.ToString() } }
                        });
                        _logger.LogWarning($"Error al procesar fila {rowNumber}: {ex.Message}");
                    }
                }

                result.TotalRows = rowNumber - 1;
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
        public async Task<CaseImportResultDto> ImportFromExcelAsync(Stream fileStream, int importedByUserId, Dictionary<string, string>? columnMapping = null)
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

                if (worksheet.RowsUsed().Count() == 0)
                {
                    throw new ArgumentException("El archivo Excel está vacío");
                }

                // Leer headers de la primera fila
                var headerRow = worksheet.Row(1);
                var headers = new List<string>();
                int lastCol = headerRow.LastCellUsed().Address.ColumnNumber;

                for (int col = 1; col <= lastCol; col++)
                {
                    headers.Add(headerRow.Cell(col).GetString().Trim());
                }

                _logger.LogInformation($"Headers detectados: {string.Join(", ", headers)}");
                _logger.LogInformation($"Mapeo recibido: {(columnMapping != null ? string.Join(", ", columnMapping.Select(kv => $"{kv.Key}={kv.Value}")) : "ninguno")}");

                var rows = worksheet.RowsUsed().Skip(1); // Saltar el header
                result.TotalRows = rows.Count();

                int rowNumber = 1;
                foreach (var row in rows)
                {
                    rowNumber++;
                    try
                    {
                        var rowData = new Dictionary<string, string>();

                        // Crear diccionario de valores por columna
                        for (int col = 1; col <= lastCol; col++)
                        {
                            var header = headers[col - 1];
                            var value = row.Cell(col).GetString().Trim();
                            rowData[header] = value;
                        }

                        var caseEntity = await MapRowToCaseEntityAsync(rowData, columnMapping, importedByUserId);
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
                            RowData = new Dictionary<string, string?> { { "Row", rowNumber.ToString() } }
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
        /// Mapea un registro CSV/Excel a una entidad Case usando mapeo dinámico de columnas
        /// </summary>
        private async Task<Case> MapRowToCaseEntityAsync(
            Dictionary<string, string> rowData,
            Dictionary<string, string>? columnMapping,
            int importedByUserId)
        {
            // Función auxiliar para obtener valor usando mapeo
            string? GetMappedValue(string systemField)
            {
                // Si no hay mapeo, usar el nombre del campo tal cual
                var columnName = columnMapping != null && columnMapping.ContainsKey(systemField)
                    ? columnMapping[systemField]
                    : systemField;

                return rowData.ContainsKey(columnName) ? rowData[columnName] : null;
            }

            // Obtener valores usando el mapeo
            var yearStr = GetMappedValue("año");
            var ageStr = GetMappedValue("edad");
            var classification = GetMappedValue("clasificacion");
            var neighborhood = GetMappedValue("barrio");
            var latitudeStr = GetMappedValue("latitud");
            var longitudeStr = GetMappedValue("longitud");

            // Parsear año
            if (string.IsNullOrWhiteSpace(yearStr) || !int.TryParse(yearStr, out int year))
            {
                throw new ArgumentException($"Año inválido o faltante: {yearStr}");
            }

            // Parsear edad
            if (string.IsNullOrWhiteSpace(ageStr) || !int.TryParse(ageStr, out int age))
            {
                throw new ArgumentException($"Edad inválida o faltante: {ageStr}");
            }

            // Validar clasificación
            if (string.IsNullOrWhiteSpace(classification))
            {
                throw new ArgumentException("Clasificación de dengue es requerida");
            }

            // Buscar tipo de dengue usando fuzzy matching
            var dengueType = await FindDengueTypeByNameAsync(classification);

            if (dengueType == null)
            {
                throw new ArgumentException($"Clasificación de dengue no encontrada: {classification}");
            }

            // Parsear y convertir coordenadas desde formato ISO
            decimal? latitude = ParseIsoCoordinate(latitudeStr);
            decimal? longitude = ParseIsoCoordinate(longitudeStr);

            // Obtener estado inicial (asumimos ID 1 = "Reportado")
            var initialStateId = 1;

            return new Case
            {
                Description = $"Caso importado - {classification}",
                Year = year,
                Age = age,
                TypeOfDengueId = dengueType.Id,
                Neighborhood = neighborhood,
                Latitude = latitude,
                Longitude = longitude,
                Address = neighborhood ?? "Sin dirección", // Usar barrio como dirección
                StateId = initialStateId,
                RegisteredByUserId = importedByUserId,
                CreatedAt = DateTime.Now,
                IsActive = true,
                // Campos opcionales que quedan null:
                // PatientId, HospitalId, MedicalStaffId, TemporaryName
            };
        }

        /// <summary>
        /// Mapea un registro CSV/Excel a una entidad Case (método legacy, mantener para compatibilidad)
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

            // Buscar tipo de dengue usando fuzzy matching
            var dengueType = await FindDengueTypeByNameAsync(dto.Classification!);

            if (dengueType == null)
            {
                throw new ArgumentException($"Clasificación de dengue no encontrada: {dto.Classification}");
            }

            // Parsear y convertir coordenadas desde formato ISO
            decimal? latitude = ParseIsoCoordinate(dto.Latitude);
            decimal? longitude = ParseIsoCoordinate(dto.Longitude);

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

        /// <summary>
        /// Busca un tipo de dengue por nombre usando fuzzy matching
        /// Implementa la misma lógica que el endpoint findByName
        /// </summary>
        private async Task<TypeOfDengue?> FindDengueTypeByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var allDengueTypes = await _dengueTypeRepository.GetAllAsync();
            var activeTypes = allDengueTypes.Where(dt => dt.IsActive).ToList();

            if (!activeTypes.Any())
            {
                return null;
            }

            var searchName = name.ToLower().Trim();

            // 1. Búsqueda de coincidencia exacta
            var exactMatch = activeTypes.FirstOrDefault(dt =>
                dt.Name.ToLower().Trim() == searchName);

            if (exactMatch != null)
            {
                _logger.LogDebug($"Coincidencia exacta: '{name}' -> '{exactMatch.Name}' (ID: {exactMatch.Id})");
                return exactMatch;
            }

            // 2. Búsqueda de coincidencia parcial (contiene)
            var partialMatch = activeTypes.FirstOrDefault(dt =>
                dt.Name.ToLower().Contains(searchName) ||
                searchName.Contains(dt.Name.ToLower()));

            if (partialMatch != null)
            {
                _logger.LogDebug($"Coincidencia parcial: '{name}' -> '{partialMatch.Name}' (ID: {partialMatch.Id})");
                return partialMatch;
            }

            // 3. Búsqueda fuzzy usando Jaccard Similarity
            var bestMatch = activeTypes
                .Select(dt => new
                {
                    Type = dt,
                    Score = CalculateSimilarity(searchName, dt.Name.ToLower())
                })
                .OrderByDescending(x => x.Score)
                .First();

            if (bestMatch.Score > 0.3) // Umbral mínimo de similitud
            {
                _logger.LogDebug($"Coincidencia fuzzy: '{name}' -> '{bestMatch.Type.Name}' (ID: {bestMatch.Type.Id}, Confianza: {Math.Round(bestMatch.Score * 100, 2)}%)");
                return bestMatch.Type;
            }

            _logger.LogWarning($"No se encontró ningún tipo de dengue que coincida con '{name}'");
            return null;
        }

        /// <summary>
        /// Calcula la similitud entre dos textos usando Jaccard Similarity
        /// </summary>
        private double CalculateSimilarity(string text1, string text2)
        {
            // Palabras clave para dengue
            var keywords = new[] { "dengue", "signos", "alarma", "grave", "muerte", "sin", "con" };

            var words1 = text1.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => keywords.Contains(w.ToLower()))
                .ToHashSet();

            var words2 = text2.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => keywords.Contains(w.ToLower()))
                .ToHashSet();

            if (words1.Count == 0 && words2.Count == 0) return 0;

            var intersection = words1.Intersect(words2).Count();
            var union = words1.Union(words2).Count();

            return union > 0 ? (double)intersection / union : 0;
        }

        /// <summary>
        /// Convierte coordenadas desde formato ISO (4.109.694.509) a formato decimal (4.109694509)
        /// También maneja formatos estándar como 4.5389 o -75.6706
        /// </summary>
        private decimal? ParseIsoCoordinate(string? coordinate)
        {
            if (string.IsNullOrWhiteSpace(coordinate))
            {
                return null;
            }

            try
            {
                // Eliminar espacios en blanco
                var cleaned = coordinate.Trim();

                // Detectar si es formato ISO (múltiples puntos)
                var dotCount = cleaned.Count(c => c == '.');

                if (dotCount > 1)
                {
                    // Formato ISO: 4.109.694.509 -> 4.109694509
                    // Mantener el primer punto (decimal) y remover los demás (separadores de miles)
                    var parts = cleaned.Split('.');
                    if (parts.Length > 0)
                    {
                        // Primer parte es la parte entera
                        var integerPart = parts[0];
                        // El resto son decimales
                        var decimalPart = string.Join("", parts.Skip(1));
                        cleaned = $"{integerPart}.{decimalPart}";
                    }
                }

                // Parsear usando cultura invariante
                if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result;
                }

                _logger.LogWarning($"No se pudo parsear la coordenada: {coordinate}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error al parsear coordenada '{coordinate}': {ex.Message}");
                return null;
            }
        }
    }
}
