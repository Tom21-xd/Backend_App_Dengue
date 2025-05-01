using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class TypeOfDengueModel
    {
        [JsonPropertyName("ID_TIPODENGUE")]
        public int ID_TIPODENGUE { get; set; }
        [JsonPropertyName("NOMBRE_TIPODENGUE")]
        public string NOMBRE_TIPODENGUE { get; set; }
        [JsonPropertyName("ESTADO_TIPODENGUE")]
        public int ESTADO_TIPODENGUE { get; set; }

    }
}
