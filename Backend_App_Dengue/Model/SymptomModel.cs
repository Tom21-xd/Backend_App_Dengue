using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class SymptomModel
    {
        [JsonPropertyName("ID_SINTOMA")]
        public int ID_SINTOMA { get; set; }
        [JsonPropertyName("NOMBRE_SINTOMA")]
        public string NOMBRE_SINTOMA { get; set; }
        [JsonPropertyName("ESTADO_SINTOMA")]
        public int ESTADO_SINTOMA { get; set; }
    }
}
