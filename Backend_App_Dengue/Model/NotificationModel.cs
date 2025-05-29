using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class NotificationModel
    {
        [JsonPropertyName("ID_NOTIFICACION")]
        public int ID_NOTIFICACION { get; set; }
        [JsonPropertyName("FECHA_NOTIFICACION")]
        public string FECHA_NOTIFICACION { get; set; }
        [JsonPropertyName("NOMBRE_TIPONOTIFICACION")]
        public string NOMBRE_TIPONOTIFICACION { get; set; }
        [JsonPropertyName("DESCRIPCION_TIPONOTIFICACION")]
        public string DESCRIPCION_TIPONOTIFICACION { get; set; }

    }
}
