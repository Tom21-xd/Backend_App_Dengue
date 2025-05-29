using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class CaseStatesModel
    {
        [JsonPropertyName("ID_ESTADOCASO")]
        public int ID_ESTADOCASO { get; set; }
        [JsonPropertyName("NOMBRE_ESTADOCASO")]
        public string NOMBRE_ESTADOCASO { get; set; }
        [JsonPropertyName("ESTADO_ESTADOCASO")]
        public string ESTADO_ESTADOCASO { get; set; }
    }
}
