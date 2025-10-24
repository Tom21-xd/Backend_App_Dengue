using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO para respuesta de autenticación con tokens
    /// Incluye datos del usuario, access token y refresh token
    /// </summary>
    public class AuthResponseDto
    {
        [JsonPropertyName("user")]
        public UserResponseDto User { get; set; } = null!;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expiresIn")]
        public int ExpiresIn { get; set; } // En segundos
    }
}
