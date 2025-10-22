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

        // New fields - Category and Priority
        [JsonPropertyName("FK_ID_CATEGORIA")]
        public int? CategoryId { get; set; }

        [JsonPropertyName("NIVEL_PRIORIDAD")]
        public string? Priority { get; set; }

        [JsonPropertyName("FIJADA")]
        public bool IsPinned { get; set; } = false;

        // Nested objects
        [JsonPropertyName("USUARIO")]
        public UserInfoDto? User { get; set; }

        [JsonPropertyName("CATEGORIA")]
        public CategoryInfoDto? Category { get; set; }

        [JsonPropertyName("ETIQUETAS")]
        public List<TagInfoDto>? Tags { get; set; }

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

        [JsonPropertyName("FECHA_NACIMIENTO_USUARIO")]
        public DateTime? BirthDate { get; set; }
    }

    /// <summary>
    /// DTO for category information embedded in publication responses
    /// </summary>
    public class CategoryInfoDto
    {
        [JsonPropertyName("ID_CATEGORIA_PUBLICACION")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_CATEGORIA")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("ICONO")]
        public string? Icon { get; set; }
    }

    /// <summary>
    /// DTO for tag information embedded in publication responses
    /// </summary>
    public class TagInfoDto
    {
        [JsonPropertyName("ID_ETIQUETA")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_ETIQUETA")]
        public string Name { get; set; } = string.Empty;
    }
}
