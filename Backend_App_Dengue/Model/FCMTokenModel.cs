using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class FCMTokenModel
    {
        [JsonPropertyName("id_usuario")]
        public int IdUsuario { get; set; }

        [JsonPropertyName("fcm_token")]
        public string FcmToken { get; set; }
    }
}
