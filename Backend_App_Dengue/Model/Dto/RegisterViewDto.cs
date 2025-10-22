using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO for registering a view on a publication
    /// </summary>
    public class RegisterViewDto
    {
        [JsonPropertyName("UserId")]
        public int UserId { get; set; }

        [JsonPropertyName("ReadTimeSeconds")]
        public int? ReadTimeSeconds { get; set; }
    }
}
