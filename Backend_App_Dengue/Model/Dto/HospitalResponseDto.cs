using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class HospitalResponseDto
    {
        [JsonPropertyName("ID_HOSPITAL")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_HOSPITAL")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("DIRECCION_HOSPITAL")]
        public string? Address { get; set; }

        [JsonPropertyName("FK_ID_MUNICIPIO")]
        public int CityId { get; set; }

        [JsonPropertyName("LATITUD_HOSPITAL")]
        public string? Latitude { get; set; }

        [JsonPropertyName("LONGITUD_HOSPITAL")]
        public string? Longitude { get; set; }

        [JsonPropertyName("IMAGEN_HOSPITAL")]
        public string? ImageId { get; set; }

        [JsonPropertyName("ESTADO_HOSPITAL")]
        public bool IsActive { get; set; }

        // Nested entity
        [JsonPropertyName("MUNICIPIO")]
        public CityInfoDto? City { get; set; }
    }
}
