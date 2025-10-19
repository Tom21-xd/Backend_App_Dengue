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

        // Interaction counters
        [JsonPropertyName("TOTAL_REACCIONES")]
        public int TotalReactions { get; set; } = 0;

        [JsonPropertyName("TOTAL_COMENTARIOS")]
        public int TotalComments { get; set; } = 0;

        [JsonPropertyName("TOTAL_VISTAS")]
        public int TotalViews { get; set; } = 0;

        [JsonPropertyName("TOTAL_GUARDADOS")]
        public int TotalSaved { get; set; } = 0;

        [JsonPropertyName("USUARIO_HA_REACCIONADO")]
        public bool UserHasReacted { get; set; } = false;

        [JsonPropertyName("USUARIO_HA_GUARDADO")]
        public bool UserHasSaved { get; set; } = false;
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
