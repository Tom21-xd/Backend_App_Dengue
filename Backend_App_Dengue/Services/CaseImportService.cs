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
        private readonly GeocodeService _geocodeService;
        private readonly ILogger<CaseImportService> _logger;

        public CaseImportService(
            AppDbContext context,
            IRepository<TypeOfDengue> dengueTypeRepository,
            GeocodeService geocodeService,
            ILogger<CaseImportService> logger)
        {
            _context = context;
            _dengueTypeRepository = dengueTypeRepository;
            _geocodeService = geocodeService;
            _logger = logger;
        }

        /// <summary>
        /// Importa casos desde un archivo CSV
        /// </summary>
        public async Task<CaseImportResultDto> ImportFromCsvAsync(Stream fileStream, int importedByUserId, Dictionary<string, string>? columnMapping = null)
        {
            _logger.LogInformation("════════════════════════════════════════════════════");
            _logger.LogInformation("🚀 INICIANDO IMPORTACIÓN CSV EN BACKEND");
            _logger.LogInformation("════════════════════════════════════════════════════");
            _logger.LogInformation($"👤 Usuario ID: {importedByUserId}");
            _logger.LogInformation($"📦 Stream size: {fileStream.Length} bytes");

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

                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation("📋 HEADERS Y MAPEO");
                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation($"🔤 Delimitador detectado: '{delimiter}'");
                _logger.LogInformation($"📊 Total de columnas: {headers.Count}");
                _logger.LogInformation($"📄 Headers: {string.Join(", ", headers)}");

                if (columnMapping != null && columnMapping.Any())
                {
                    _logger.LogInformation($"🗺️  Mapeo recibido ({columnMapping.Count} campos):");
                    foreach (var kv in columnMapping)
                    {
                        _logger.LogInformation($"   ✓ {kv.Key} → {kv.Value}");
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️  NO SE RECIBIÓ MAPEO DE COLUMNAS");
                }

                int rowNumber = 1;
                string? line;
                var importedCases = new List<Case>();

                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation("📖 PROCESANDO FILAS");
                _logger.LogInformation("────────────────────────────────────────────────────");

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

                        // Log de la primera fila para debugging
                        if (rowNumber == 2)
                        {
                            _logger.LogInformation($"📝 Ejemplo de fila #{rowNumber}:");
                            foreach (var kv in rowData.Take(5))
                            {
                                _logger.LogInformation($"   {kv.Key} = {kv.Value}");
                            }
                            if (rowData.Count > 5)
                            {
                                _logger.LogInformation($"   ... y {rowData.Count - 5} columnas más");
                            }
                        }

                        var caseEntity = await MapRowToCaseEntityAsync(rowData, columnMapping, importedByUserId);
                        _context.Cases.Add(caseEntity);
                        importedCases.Add(caseEntity);
                        result.SuccessfulImports++;

                        // Log cada 10 casos procesados
                        if (result.SuccessfulImports % 10 == 0)
                        {
                            _logger.LogInformation($"✅ Procesados {result.SuccessfulImports} casos...");
                        }
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
                        _logger.LogWarning($"❌ Error en fila {rowNumber}: {ex.Message}");
                    }
                }

                result.TotalRows = rowNumber - 1;

                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation("💾 GUARDANDO EN BASE DE DATOS");
                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation($"📊 Total filas procesadas: {result.TotalRows}");
                _logger.LogInformation($"✅ Exitosas: {result.SuccessfulImports}");
                _logger.LogInformation($"❌ Fallidas: {result.FailedImports}");
                _logger.LogInformation($"📦 Casos en memoria: {importedCases.Count}");

                var savedCount = await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ SaveChangesAsync completado - {savedCount} entidades guardadas");

                // Llenar lista de casos importados con sus coordenadas
                _logger.LogInformation("────────────────────────────────────────────────────");
                _logger.LogInformation("📍 PREPARANDO LISTA DE CASOS IMPORTADOS");
                _logger.LogInformation("────────────────────────────────────────────────────");

                // Actualizar el contador real basado en los casos guardados
                result.SuccessfulImports = importedCases.Count;

                // Cargar los tipos de dengue para los casos importados
                var dengueTypeIds = importedCases.Select(c => c.TypeOfDengueId).Distinct().ToList();
                var dengueTypes = await _context.Set<TypeOfDengue>()
                    .Where(t => dengueTypeIds.Contains(t.Id))
                    .ToDictionaryAsync(t => t.Id, t => t.Name);

                result.ImportedCases = importedCases.Select(c => new ImportedCaseDto
                {
                    CaseId = c.Id,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    Neighborhood = c.Neighborhood,
                    TemporaryName = c.TemporaryName,
                    Year = c.Year,
                    Age = c.Age,
                    DengueType = dengueTypes.ContainsKey(c.TypeOfDengueId) ? dengueTypes[c.TypeOfDengueId] : "Desconocido"
                }).ToList();

                _logger.LogInformation($"📦 ImportedCases generado con {result.ImportedCases.Count} elementos");
                _logger.LogInformation($"📍 Casos con coordenadas: {result.ImportedCases.Count(c => c.Latitude.HasValue && c.Longitude.HasValue)}");

                // Log de los primeros 3 casos para debugging
                foreach (var importedCase in result.ImportedCases.Take(3))
                {
                    _logger.LogInformation($"Caso ID {importedCase.CaseId}:");
                    _logger.LogInformation($"   Lat: {importedCase.Latitude}, Lng: {importedCase.Longitude}");
                    _logger.LogInformation($"   Barrio: {importedCase.Neighborhood}, Nombre: {importedCase.TemporaryName}");
                    _logger.LogInformation($"   Año: {importedCase.Year}, Edad: {importedCase.Age}, Tipo: {importedCase.DengueType}");
                }

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("════════════════════════════════════════════════════");
                _logger.LogInformation("✅ IMPORTACIÓN COMPLETADA EXITOSAMENTE");
                _logger.LogInformation("════════════════════════════════════════════════════");
                _logger.LogInformation($"⏱️  Tiempo total: {result.ProcessingTime.TotalSeconds:F2} segundos");
                _logger.LogInformation($"📊 Resumen: {result.SuccessfulImports}/{result.TotalRows} casos importados");
                _logger.LogInformation($"📦 Casos con coordenadas: {result.ImportedCases.Count(c => c.Latitude.HasValue && c.Longitude.HasValue)}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("════════════════════════════════════════════════════");
                _logger.LogError("💥 ERROR CRÍTICO EN IMPORTACIÓN CSV");
                _logger.LogError("════════════════════════════════════════════════════");
                _logger.LogError(ex, $"Tipo: {ex.GetType().Name}, Mensaje: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
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
                var importedCases = new List<Case>();

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
                        importedCases.Add(caseEntity);
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

                // Llenar lista de casos importados con sus coordenadas
                result.ImportedCases = importedCases.Select(c => new ImportedCaseDto
                {
                    CaseId = c.Id,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    Neighborhood = c.Neighborhood,
                    TemporaryName = c.TemporaryName,
                    Year = c.Year,
                    Age = c.Age,
                    DengueType = c.TypeOfDengue?.Name ?? "Desconocido"
                }).ToList();

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

        /// <summary>
        /// Importa casos con mapeo personalizado de columnas permitiendo múltiples columnas por campo
        /// </summary>
        public async Task<CaseImportResultDto> ImportWithCustomMappingAsync(
            Stream fileStream,
            int importedByUserId,
            ColumnMappingDto columnMappingDto,
            bool isExcel = false)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new CaseImportResultDto
            {
                ImportedAt = DateTime.Now,
                ImportedByUserId = importedByUserId
            };

            try
            {
                List<Dictionary<string, string>> rows;

                if (isExcel)
                {
                    rows = ReadExcelRows(fileStream);
                }
                else
                {
                    rows = await ReadCsvRowsAsync(fileStream);
                }

                result.TotalRows = rows.Count;

                int rowNumber = 1;
                var importedCases = new List<Case>();

                foreach (var rowData in rows)
                {
                    rowNumber++;
                    try
                    {
                        var caseEntity = await MapRowToCaseWithMappingAsync(rowData, columnMappingDto, importedByUserId, rowNumber);
                        _context.Cases.Add(caseEntity);
                        importedCases.Add(caseEntity);
                        result.SuccessfulImports++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedImports++;
                        result.Errors.Add(new CaseImportErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = ex.Message,
                            RowData = rowData
                        });
                        _logger.LogWarning($"Error al procesar fila {rowNumber}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                // Llenar lista de casos importados con sus coordenadas
                result.ImportedCases = importedCases.Select(c => new ImportedCaseDto
                {
                    CaseId = c.Id,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    Neighborhood = c.Neighborhood,
                    TemporaryName = c.TemporaryName,
                    Year = c.Year,
                    Age = c.Age,
                    DengueType = c.TypeOfDengue?.Name ?? "Desconocido"
                }).ToList();

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation($"Importación completada: {result.SuccessfulImports}/{result.TotalRows} casos importados");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante la importación");
                throw;
            }
        }

        /// <summary>
        /// Importa casos con mapeo personalizado de columnas (CSV/Excel) - Versión simple
        /// </summary>
        public async Task<CaseImportResultDto> ImportWithCustomMappingAsync(
            Stream fileStream,
            int importedByUserId,
            Dictionary<string, string> columnMapping,
            bool isExcel = false)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new CaseImportResultDto
            {
                ImportedAt = DateTime.Now,
                ImportedByUserId = importedByUserId
            };

            try
            {
                List<Dictionary<string, string>> rows;

                if (isExcel)
                {
                    rows = ReadExcelRows(fileStream);
                }
                else
                {
                    rows = await ReadCsvRowsAsync(fileStream);
                }

                result.TotalRows = rows.Count;

                int rowNumber = 1;
                var importedCases = new List<Case>();

                foreach (var rowData in rows)
                {
                    rowNumber++;
                    try
                    {
                        var caseEntity = await MapRowToCaseWithMappingAsync(rowData, columnMapping, importedByUserId, rowNumber);
                        _context.Cases.Add(caseEntity);
                        importedCases.Add(caseEntity);
                        result.SuccessfulImports++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedImports++;
                        result.Errors.Add(new CaseImportErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = ex.Message,
                            RowData = rowData
                        });
                        _logger.LogWarning($"Error al procesar fila {rowNumber}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                // Llenar lista de casos importados con sus coordenadas
                result.ImportedCases = importedCases.Select(c => new ImportedCaseDto
                {
                    CaseId = c.Id,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    Neighborhood = c.Neighborhood,
                    TemporaryName = c.TemporaryName,
                    Year = c.Year,
                    Age = c.Age,
                    DengueType = c.TypeOfDengue?.Name ?? "Desconocido"
                }).ToList();

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation($"Importación completada: {result.SuccessfulImports}/{result.TotalRows} casos importados");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante la importación");
                throw;
            }
        }

        /// <summary>
        /// Mapea una fila de datos a una entidad Case usando mapeo con múltiples columnas
        /// </summary>
        private async Task<Case> MapRowToCaseWithMappingAsync(
            Dictionary<string, string> rowData,
            ColumnMappingDto columnMappingDto,
            int importedByUserId,
            int rowNumber)
        {
            // Función auxiliar para obtener valor mapeado (concatena múltiples columnas si es necesario)
            string? GetMappedValue(string systemField)
            {
                if (!columnMappingDto.Mapping.ContainsKey(systemField))
                    return null;

                var columnNames = columnMappingDto.Mapping[systemField];
                var values = columnNames
                    .Select(colName => rowData.ContainsKey(colName) ? rowData[colName]?.Trim() : null)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();

                if (values.Count == 0)
                    return null;

                // Concatenar con el separador configurado
                return string.Join(columnMappingDto.Separator, values);
            }

            // Reutilizar la misma lógica del método original
            return await MapRowToCaseInternalAsync(rowData, GetMappedValue, importedByUserId, rowNumber);
        }

        /// <summary>
        /// Mapea una fila de datos a una entidad Case usando mapeo personalizado simple
        /// </summary>
        private async Task<Case> MapRowToCaseWithMappingAsync(
            Dictionary<string, string> rowData,
            Dictionary<string, string> columnMapping,
            int importedByUserId,
            int rowNumber)
        {
            // Función auxiliar para obtener valor mapeado
            string? GetMappedValue(string systemField)
            {
                if (!columnMapping.ContainsKey(systemField))
                    return null;

                var columnName = columnMapping[systemField];
                return rowData.ContainsKey(columnName) ? rowData[columnName]?.Trim() : null;
            }

            // Reutilizar la misma lógica
            return await MapRowToCaseInternalAsync(rowData, GetMappedValue, importedByUserId, rowNumber);
        }

        /// <summary>
        /// Lógica interna compartida para mapear una fila a Case
        /// </summary>
        private async Task<Case> MapRowToCaseInternalAsync(
            Dictionary<string, string> rowData,
            Func<string, string?> getMappedValue,
            int importedByUserId,
            int rowNumber)
        {

            // 1. CAMPOS OBLIGATORIOS

            // Año
            var yearStr = getMappedValue("año");
            if (string.IsNullOrWhiteSpace(yearStr) || !int.TryParse(yearStr, out int year))
            {
                throw new ArgumentException($"Año inválido o faltante: {yearStr}");
            }

            // Edad
            var ageStr = getMappedValue("edad");
            if (string.IsNullOrWhiteSpace(ageStr) || !int.TryParse(ageStr, out int age))
            {
                throw new ArgumentException($"Edad inválida o faltante: {ageStr}");
            }

            // Clasificación de dengue
            var classification = getMappedValue("clasificacion");
            if (string.IsNullOrWhiteSpace(classification))
            {
                throw new ArgumentException("Clasificación de dengue es requerida");
            }

            // Normalizar clasificación: "DENGUE" → "Dengue sin signos de alarma"
            var normalizedClassification = NormalizeDengueClassification(classification);
            var dengueType = await FindDengueTypeByNameAsync(normalizedClassification);

            if (dengueType == null)
            {
                throw new ArgumentException($"Clasificación de dengue no encontrada: {classification}");
            }

            // Barrio (obligatorio)
            var neighborhood = getMappedValue("barrio");
            if (string.IsNullOrWhiteSpace(neighborhood))
            {
                throw new ArgumentException("Barrio es requerido");
            }

            // 2. COORDENADAS O DIRECCIÓN (OBLIGATORIO)
            // Recopilar todas las posibles fuentes de datos para geocodificación

            var cityOptions = new List<string?>
            {
                getMappedValue("ciudad"),
                getMappedValue("ciudad2"),
                getMappedValue("municipio")
            };

            var neighborhoodOptions = new List<string?>
            {
                neighborhood, // Ya validado como obligatorio arriba
                getMappedValue("barrio2"),
                getMappedValue("vereda")
            };

            var addressOptions = new List<string?>
            {
                getMappedValue("direccion"),
                getMappedValue("direccion2"),
                getMappedValue("dir_res")
            };

            var latitudeOptions = new List<string?>
            {
                getMappedValue("latitud"),
                getMappedValue("lat"),
                getMappedValue("latitude")
            };

            var longitudeOptions = new List<string?>
            {
                getMappedValue("longitud"),
                getMappedValue("lon"),
                getMappedValue("lng"),
                getMappedValue("longitude")
            };

            // Llamar servicio inteligente de geocodificación
            var (latitude, longitude) = await _geocodeService.GetCoordinatesFromMultipleSourcesAsync(
                cityOptions,
                neighborhoodOptions,
                addressOptions,
                latitudeOptions,
                longitudeOptions);

            // Las coordenadas pueden ser null si la geocodificación falló
            // En ese caso, se guardarán como NULL en la base de datos para que el usuario las corrija manualmente

            // 3. CAMPOS OPCIONALES

            var temporaryName = getMappedValue("nombre"); // Opcional
            var address = getMappedValue("direccion") ?? neighborhood; // Usar barrio si no hay dirección

            // 4. CREAR ENTIDAD CASE

            return new Case
            {
                Description = $"Caso importado - {classification}",
                Year = year,
                Age = age,
                TypeOfDengueId = dengueType.Id,
                Neighborhood = neighborhood,
                Address = address,
                Latitude = latitude,
                Longitude = longitude,
                TemporaryName = temporaryName,
                StateId = 1, // Estado inicial: Reportado
                RegisteredByUserId = importedByUserId,
                CreatedAt = DateTime.Now,
                IsActive = true,
                // NULL por defecto
                PatientId = null,
                HospitalId = null,
                MedicalStaffId = null
            };
        }

        /// <summary>
        /// Normaliza la clasificación de dengue al formato del sistema
        /// </summary>
        private string NormalizeDengueClassification(string classification)
        {
            var normalized = classification.Trim().ToUpperInvariant();

            // Mapeo: "DENGUE" sin especificar → asumimos sin signos de alarma
            if (normalized == "DENGUE")
            {
                return "Dengue sin signos de alarma";
            }

            // Para otros casos, retornar con capitalización correcta
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                classification.ToLower());
        }

        /// <summary>
        /// Lee filas de un archivo CSV
        /// </summary>
        private async Task<List<Dictionary<string, string>>> ReadCsvRowsAsync(Stream fileStream)
        {
            var rows = new List<Dictionary<string, string>>();

            using var reader = new StreamReader(fileStream);

            // Leer encabezados
            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(headerLine))
            {
                throw new ArgumentException("El archivo CSV está vacío");
            }

            // Detectar delimitador
            var delimiter = headerLine.Count(c => c == ';') > headerLine.Count(c => c == ',') ? ";" : ",";
            var headers = headerLine.Split(delimiter).Select(h => h.Trim().Trim('"')).ToList();

            // Leer filas de datos
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(delimiter).Select(v => v.Trim().Trim('"')).ToArray();
                var rowData = new Dictionary<string, string>();

                for (int i = 0; i < Math.Min(headers.Count, values.Length); i++)
                {
                    rowData[headers[i]] = values[i];
                }

                rows.Add(rowData);
            }

            return rows;
        }

        /// <summary>
        /// Lee filas de un archivo Excel
        /// </summary>
        private List<Dictionary<string, string>> ReadExcelRows(Stream fileStream)
        {
            var rows = new List<Dictionary<string, string>>();

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);

            if (worksheet.RowsUsed().Count() == 0)
            {
                throw new ArgumentException("El archivo Excel está vacío");
            }

            // Leer headers
            var headerRow = worksheet.Row(1);
            var headers = new List<string>();
            int lastCol = headerRow.LastCellUsed().Address.ColumnNumber;

            for (int col = 1; col <= lastCol; col++)
            {
                headers.Add(headerRow.Cell(col).GetString().Trim());
            }

            // Leer filas de datos
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var rowData = new Dictionary<string, string>();

                for (int col = 1; col <= lastCol; col++)
                {
                    var header = headers[col - 1];
                    var value = row.Cell(col).GetString().Trim();
                    rowData[header] = value;
                }

                rows.Add(rowData);
            }

            return rows;
        }

        /// <summary>
        /// Valida las columnas del archivo y retorna información para el mapeo
        /// </summary>
        public async Task<ColumnValidationResultDto> ValidateColumnsAsync(
            Stream fileStream,
            bool isExcel = false)
        {
            var result = new ColumnValidationResultDto();

            try
            {
                List<string> detectedColumns;

                if (isExcel)
                {
                    using var workbook = new XLWorkbook(fileStream);
                    var worksheet = workbook.Worksheet(1);
                    var headerRow = worksheet.Row(1);
                    int lastCol = headerRow.LastCellUsed().Address.ColumnNumber;

                    detectedColumns = new List<string>();
                    for (int col = 1; col <= lastCol; col++)
                    {
                        detectedColumns.Add(headerRow.Cell(col).GetString().Trim());
                    }
                }
                else
                {
                    using var reader = new StreamReader(fileStream);
                    var headerLine = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(headerLine))
                    {
                        throw new ArgumentException("El archivo está vacío");
                    }

                    var delimiter = headerLine.Count(c => c == ';') > headerLine.Count(c => c == ',') ? ";" : ",";
                    detectedColumns = headerLine.Split(delimiter)
                        .Select(h => h.Trim().Trim('"'))
                        .ToList();
                }

                result.DetectedColumns = detectedColumns;
                result.IsValid = true;

                // Sugerir mapeo automático
                result.SuggestedMapping = GenerateSuggestedMapping(detectedColumns);

                _logger.LogInformation($"Validación de columnas exitosa. Detectadas: {string.Join(", ", detectedColumns)}");
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error al validar columnas del archivo");
            }

            return result;
        }

        /// <summary>
        /// Genera mapeo sugerido basado en las columnas detectadas
        /// </summary>
        private Dictionary<string, string?> GenerateSuggestedMapping(List<string> columns)
        {
            var mapping = new Dictionary<string, string?>();

            // Mapeo inteligente basado en nombres comunes
            var patterns = new Dictionary<string, string[]>
            {
                ["año"] = new[] { "año", "año_", "anio", "year" },
                ["edad"] = new[] { "edad", "edad_", "age" },
                ["clasificacion"] = new[] { "clasificacion del caso", "nom_eve", "tipo_dengue", "clasificacion" },
                ["barrio"] = new[] { "bar_ver_", "barrio", "neighborhood", "vereda" },
                ["ciudad"] = new[] { "nmun_resi", "ciudad", "municipio", "city" },
                ["latitud"] = new[] { "lat_dir", "latitud", "latitude", "lat" },
                ["longitud"] = new[] { "long_dir", "longitud", "longitude", "lng", "lon" },
                ["nombre"] = new[] { "nombre_completo", "nombre", "name", "paciente" },
                ["direccion"] = new[] { "dir_res_", "direccion", "address" }
            };

            foreach (var field in patterns.Keys)
            {
                var matchedColumn = columns.FirstOrDefault(col =>
                    patterns[field].Any(pattern =>
                        col.ToLowerInvariant().Contains(pattern.ToLowerInvariant())));

                mapping[field] = matchedColumn;
            }

            return mapping;
        }
    }
}
