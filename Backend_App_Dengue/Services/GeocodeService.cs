using System.Globalization;
using System.Text.Json;

namespace Backend_App_Dengue.Services
{
    /// <summary>
    /// Servicio para geocodificación: convertir dirección/ciudad/barrio en coordenadas
    /// </summary>
    public class GeocodeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeocodeService> _logger;

        public GeocodeService(HttpClient httpClient, ILogger<GeocodeService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Busca coordenadas de forma inteligente combinando múltiples fuentes de datos
        /// Prioridad: coordenadas explícitas > geocodificación online usando Nominatim
        /// </summary>
        public async Task<(decimal? lat, decimal? lng)> GetCoordinatesFromMultipleSourcesAsync(
            List<string?> cityOptions,
            List<string?> neighborhoodOptions,
            List<string?> addressOptions,
            List<string?> latitudeOptions,
            List<string?> longitudeOptions)
        {
            // 1. PRIORIDAD MÁXIMA: Coordenadas explícitas
            foreach (var latStr in latitudeOptions.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                foreach (var lngStr in longitudeOptions.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    var lat = ParseCoordinate(latStr);
                    var lng = ParseCoordinate(lngStr);

                    if (lat.HasValue && lng.HasValue)
                    {
                        _logger.LogInformation($"📍 Usando coordenadas del archivo: {lat}, {lng}");
                        return (lat, lng);
                    }
                }
            }

            _logger.LogInformation("🌐 No hay coordenadas en el archivo, intentando Nominatim...");

            // 2. Intentar geocodificación online usando Nominatim
            // Probar con cada combinación de ciudad disponible
            foreach (var city in cityOptions.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var firstNeighborhood = neighborhoodOptions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                var firstAddress = addressOptions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                _logger.LogInformation($"🔍 Buscando en Nominatim: Ciudad={city}, Barrio={firstNeighborhood}");

                var onlineResult = await GeocodeOnlineAsync(city!, firstNeighborhood, firstAddress);
                if (onlineResult.HasValue)
                {
                    _logger.LogInformation($"✅ Nominatim encontró: {onlineResult.Value.lat}, {onlineResult.Value.lng}");
                    return onlineResult.Value;
                }
            }

            // 3. No se pudo geocodificar
            var firstCity = cityOptions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            var firstNeighborhood2 = neighborhoodOptions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            _logger.LogWarning($"❌ Geocodificación fallida. Ciudad: {firstCity}, Barrio: {firstNeighborhood2}");
            return (null, null);
        }

        /// <summary>
        /// Obtiene coordenadas a partir de ciudad, barrio y dirección (método simple)
        /// </summary>
        public async Task<(decimal? lat, decimal? lng)> GetCoordinatesAsync(
            string? city,
            string? neighborhood = null,
            string? address = null,
            string? latitudeStr = null,
            string? longitudeStr = null)
        {
            // 1. Si ya vienen coordenadas explícitas, usarlas directamente
            if (!string.IsNullOrWhiteSpace(latitudeStr) && !string.IsNullOrWhiteSpace(longitudeStr))
            {
                var lat = ParseCoordinate(latitudeStr);
                var lng = ParseCoordinate(longitudeStr);

                if (lat.HasValue && lng.HasValue)
                {
                    _logger.LogDebug($"Usando coordenadas explícitas: {lat}, {lng}");
                    return (lat, lng);
                }
            }

            // 2. Si no hay ciudad, retornar null
            if (string.IsNullOrWhiteSpace(city))
            {
                _logger.LogWarning("No se proporcionó ciudad para geocodificar");
                return (null, null);
            }

            // 3. Intentar geocodificación online usando Nominatim
            var onlineCoords = await GeocodeOnlineAsync(city, neighborhood, address);
            if (onlineCoords.HasValue)
            {
                return onlineCoords.Value;
            }

            // 4. No se pudo geocodificar
            _logger.LogWarning("No se pudieron obtener coordenadas para ciudad: {City}, barrio: {Neighborhood}", city, neighborhood);
            return (null, null);
        }

        /// <summary>
        /// Geocodifica usando Nominatim (OpenStreetMap API)
        /// Estrategia progresiva: intenta con más detalle primero, luego va simplificando
        /// SIEMPRE debe retornar algo si al menos la ciudad es válida
        /// </summary>
        private async Task<(decimal lat, decimal lng)?> GeocodeOnlineAsync(
            string city,
            string? neighborhood,
            string? address)
        {
            try
            {
                // ESTRATEGIA 1: Dirección completa + ciudad + departamento
                if (!string.IsNullOrWhiteSpace(address))
                {
                    var fullQuery = $"{address}, {city}, Valle del Cauca, Colombia";
                    var fullResult = await TryGeocodeQueryAsync(fullQuery);
                    if (fullResult.HasValue)
                    {
                        _logger.LogInformation("🎯 Geocodificación precisa: dirección completa");
                        return fullResult;
                    }

                    // Intentar solo dirección + ciudad (sin departamento)
                    var simpleQuery = $"{address}, {city}, Colombia";
                    var simpleResult = await TryGeocodeQueryAsync(simpleQuery);
                    if (simpleResult.HasValue)
                    {
                        _logger.LogInformation("🎯 Geocodificación: dirección + ciudad");
                        return simpleResult;
                    }
                }

                // ESTRATEGIA 2: Barrio + ciudad
                if (!string.IsNullOrWhiteSpace(neighborhood))
                {
                    var neighborhoodQuery = $"{neighborhood}, {city}, Valle del Cauca, Colombia";
                    var neighborhoodResult = await TryGeocodeQueryAsync(neighborhoodQuery);
                    if (neighborhoodResult.HasValue)
                    {
                        _logger.LogInformation("🎯 Geocodificación: barrio + ciudad");
                        return neighborhoodResult;
                    }

                    // Intentar barrio + ciudad sin departamento
                    var simpleNeighborhoodQuery = $"{neighborhood}, {city}, Colombia";
                    var simpleNeighborhoodResult = await TryGeocodeQueryAsync(simpleNeighborhoodQuery);
                    if (simpleNeighborhoodResult.HasValue)
                    {
                        _logger.LogInformation("🎯 Geocodificación: barrio + ciudad (simple)");
                        return simpleNeighborhoodResult;
                    }
                }

                // ESTRATEGIA 3: Ciudad + Valle del Cauca
                var cityValleQuery = $"{city}, Valle del Cauca, Colombia";
                var cityValleResult = await TryGeocodeQueryAsync(cityValleQuery);
                if (cityValleResult.HasValue)
                {
                    _logger.LogWarning("📍 Geocodificación a nivel de ciudad (Valle del Cauca): {City}", city);
                    return cityValleResult;
                }

                // ESTRATEGIA 4: Solo ciudad + Colombia (más flexible)
                var cityColombiaQuery = $"{city}, Colombia";
                var cityColombiaResult = await TryGeocodeQueryAsync(cityColombiaQuery);
                if (cityColombiaResult.HasValue)
                {
                    _logger.LogWarning("📍 Geocodificación a nivel de ciudad (Colombia): {City}", city);
                    return cityColombiaResult;
                }

                // ESTRATEGIA 5: Solo la ciudad (último recurso)
                var justCityResult = await TryGeocodeQueryAsync(city);
                if (justCityResult.HasValue)
                {
                    _logger.LogWarning("📍 Geocodificación solo por nombre de ciudad: {City}", city);
                    return justCityResult;
                }

                _logger.LogError("❌ No se encontró ninguna coincidencia en Nominatim para: Ciudad={City}, Barrio={Neighborhood}, Dirección={Address}",
                    city, neighborhood ?? "N/A", address ?? "N/A");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "🔴 Error de conexión con Nominatim API");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "🔴 Error al parsear respuesta de Nominatim");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔴 Error en geocodificación online");
            }

            return null;
        }

        /// <summary>
        /// Intenta geocodificar una query específica usando Nominatim
        /// </summary>
        private async Task<(decimal lat, decimal lng)?> TryGeocodeQueryAsync(string query)
        {
            try
            {
                // Configurar headers requeridos por Nominatim
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "DengueApp/1.0 (UCEVA)");

                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";

                _logger.LogInformation("🌍 Nominatim query: {Query}", query);

                // Respetar rate limit de Nominatim (máximo 1 request por segundo)
                await Task.Delay(1100);

                var response = await _httpClient.GetStringAsync(url);
                var results = JsonSerializer.Deserialize<List<NominatimResult>>(response);

                if (results != null && results.Count > 0)
                {
                    var result = results[0];
                    _logger.LogInformation("✅ Nominatim respuesta: {Query} -> {Lat}, {Lon}", query, result.Lat, result.Lon);
                    return (decimal.Parse(result.Lat, CultureInfo.InvariantCulture),
                            decimal.Parse(result.Lon, CultureInfo.InvariantCulture));
                }
                else
                {
                    _logger.LogWarning("⚠️ Nominatim sin resultados para: {Query}", query);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "🔴 Error HTTP en Nominatim: {Query}", query);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "🔴 Timeout en Nominatim: {Query}", query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔴 Error en Nominatim: {Query}", query);
            }

            return null;
        }

        /// <summary>
        /// Parsea una coordenada en formato string a decimal
        /// Soporta formato ISO (4.109.694.509) y estándar (4.5389)
        /// </summary>
        private decimal? ParseCoordinate(string? coordinate)
        {
            if (string.IsNullOrWhiteSpace(coordinate))
                return null;

            try
            {
                var cleaned = coordinate.Trim();

                // Detectar si es formato ISO (múltiples puntos)
                var dotCount = cleaned.Count(c => c == '.');

                if (dotCount > 1)
                {
                    // Formato ISO: 4.109.694.509 -> 4.109694509
                    var parts = cleaned.Split('.');
                    if (parts.Length > 0)
                    {
                        var integerPart = parts[0];
                        var decimalPart = string.Join("", parts.Skip(1));
                        cleaned = $"{integerPart}.{decimalPart}";
                    }
                }

                // Parsear
                if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result;
                }

                _logger.LogWarning($"No se pudo parsear coordenada: {coordinate}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error al parsear coordenada '{coordinate}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Clase para deserializar respuesta de Nominatim
        /// </summary>
        private class NominatimResult
        {
            public string Lat { get; set; } = string.Empty;
            public string Lon { get; set; } = string.Empty;
        }
    }
}
