using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class TypeOfBloodModel
    {
        [JsonPropertyName("ID_TIPOSANGRE")]
        public int ID_TIPOSANGRE { get; set; }
        [JsonPropertyName("NOMBRE_TIPOSANGRE")]
        public string NOMBRE_TIPOSANGRE { get; set; }
        [JsonPropertyName("ESTADO_TIPOSANGRE")]
        public int ESTADO_TIPOSANGRE { get; set; }

    }
}
