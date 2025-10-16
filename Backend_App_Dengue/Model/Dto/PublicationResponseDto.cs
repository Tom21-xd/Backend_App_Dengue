using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO for Publication with nested User object
    /// </summary>
    public class PublicationResponseDto
    {
        [JsonPropertyName("ID_PUBLICACION")]
        public int Id { get; set; }

        [JsonPropertyName("TITULO_PUBLICACION")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("DESCRIPCION_PUBLICACION")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("FECHA_PUBLICACION")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("FK_ID_USUARIO")]
        public int UserId { get; set; }

        [JsonPropertyName("IMAGEN_PUBLICACION")]
        public string? ImageId { get; set; }

        [JsonPropertyName("ESTADO_PUBLICACION")]
        public bool IsActive { get; set; }

        // Nested user object
        [JsonPropertyName("USUARIO")]
        public UserInfoDto? User { get; set; }
    }

    /// <summary>
    /// DTO for basic user information embedded in other responses
    /// </summary>
    public class UserInfoDto
    {
        [JsonPropertyName("ID_USUARIO")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_USUARIO")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("CORREO_USUARIO")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("NOMBRE_ROL")]
        public string? RoleName { get; set; }
    }
}
