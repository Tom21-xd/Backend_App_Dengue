using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class NotificationResponseDto
    {
        [JsonPropertyName("ID_NOTIFICACION")]
        public int Id { get; set; }

        [JsonPropertyName("CONTENIDO_NOTIFICACION")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("FECHA_NOTIFICACION")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [JsonPropertyName("LEIDA_NOTIFICACION")]
        public bool IsRead { get; set; }

        [JsonPropertyName("ESTADO_NOTIFICACION")]
        public bool IsActive { get; set; }

        // Nested entity
        [JsonPropertyName("USUARIO")]
        public UserInfoDto? User { get; set; }
    }
}
