using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class UpdatePublicationDto
    {
        [JsonPropertyName("titulo")]
        public string? Titulo { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }
    }
}
