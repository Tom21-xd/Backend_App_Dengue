using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class CreateHospitalDto
    {
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("direccion")]
        public string Direccion { get; set; }

        [JsonPropertyName("latitud")]
        public string Latitud { get; set; }

        [JsonPropertyName("longitud")]
        public string Longitud { get; set; }

        [JsonPropertyName("id_municipio")]
        public int IdMunicipio { get; set; }

        [JsonPropertyName("imagen")]
        public IFormFile? Imagen { get; set; }
    }
}
