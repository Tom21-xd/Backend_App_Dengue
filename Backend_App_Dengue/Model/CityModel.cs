using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class CityModel
    {
        [JsonPropertyName("ID_MUNICIPIO")]
        public int ID_MUNICIPIO { get; set; }

        [JsonPropertyName("NOMBRE_MUNICIPIO")]
        public string NOMBRE_MUNICIPIO { get; set; }

        [JsonPropertyName("FK_ID_DEPARTAMENTO")]
        public int FK_ID_DEPARTAMENTO { get; set; }
    }
}
