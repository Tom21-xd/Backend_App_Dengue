using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO para solicitar renovación de access token
    /// </summary>
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "El refresh token es requerido")]
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("deviceInfo")]
        public string? DeviceInfo { get; set; }
    }
}
