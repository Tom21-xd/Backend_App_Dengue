using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class GenreModel
    {
        [JsonPropertyName("ID_GENERO")]
        public int ID_GENERO { get; set; }
        [JsonPropertyName("NOMBRE_GENERO")]
        public string NOMBRE_GENERO { get; set; }
        [JsonPropertyName("ESTADO_GENERO")]
        public int ESTADO_GENERO { get; set; }
    }
}
