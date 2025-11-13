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
                        _logger.LogDebug($"Coordenadas explícitas encontradas: {lat}, {lng}");
                        return (lat, lng);
                    }
                }
            }

            // 2. Intentar geocodificación online usando Nominatim
            // Probar con cada combinación de ciudad disponible
            foreach (var city in cityOptions.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var firstNeighborhood = neighborhoodOptions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                var firstAddress = addressOptions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                var onlineResult = await GeocodeOnlineAsync(city!, firstNeighborhood, firstAddress);
                if (onlineResult.HasValue)
                {
                    return onlineResult.Value;
                }
            }

            // 3. No se pudo geocodificar
            var firstCity = cityOptions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            var firstNeighborhood2 = neighborhoodOptions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            _logger.LogWarning($"No se pudieron obtener coordenadas. Ciudad: {firstCity}, Barrio: {firstNeighborhood2}");
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
        /// </summary>
        private async Task<(decimal lat, decimal lng)?> GeocodeOnlineAsync(
            string city,
            string? neighborhood,
            string? address)
        {
            try
            {
                // Construir query de búsqueda
                var searchParts = new List<string>();

                if (!string.IsNullOrWhiteSpace(address))
                    searchParts.Add(address);

                if (!string.IsNullOrWhiteSpace(neighborhood))
                    searchParts.Add(neighborhood);

                searchParts.Add(city);
                searchParts.Add("Valle del Cauca");
                searchParts.Add("Colombia");

                var query = string.Join(", ", searchParts);

                // Llamar a Nominatim (respetar política de uso: User-Agent requerido)
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "DengueApp/1.0 (UCEVA)");

                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";

                _logger.LogDebug("Geocodificando: {Query}", query);

                // Respetar rate limit de Nominatim (máximo 1 request por segundo)
                await Task.Delay(1000);

                var response = await _httpClient.GetStringAsync(url);
                var results = JsonSerializer.Deserialize<List<NominatimResult>>(response);

                if (results != null && results.Count > 0)
                {
                    var result = results[0];
                    _logger.LogInformation("Geocodificación exitosa: {Query} -> {Lat}, {Lon}", query, result.Lat, result.Lon);
                    return (decimal.Parse(result.Lat, CultureInfo.InvariantCulture),
                            decimal.Parse(result.Lon, CultureInfo.InvariantCulture));
                }
                else
                {
                    _logger.LogWarning("No se encontraron resultados para: {Query}", query);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión con Nominatim API");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta de Nominatim");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en geocodificación online");
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
