using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class UpdateHospitalDto
    {
        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("imagen_id")]
        public string? ImagenId { get; set; }
    }
}
