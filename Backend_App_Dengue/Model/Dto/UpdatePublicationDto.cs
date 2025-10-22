using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class UpdatePublicationDto
    {
        [JsonPropertyName("titulo")]
        public string? Titulo { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("categoriaId")]
        public int? CategoriaId { get; set; }

        [JsonPropertyName("etiquetasIds")]
        public string? EtiquetasIds { get; set; } // Comma-separated IDs: "1,2,3"

        [JsonPropertyName("prioridad")]
        public string? Prioridad { get; set; } // "Baja" | "Normal" | "Alta" | "Urgente"

        [JsonPropertyName("fijada")]
        public bool? Fijada { get; set; }

        [JsonPropertyName("latitud")]
        public double? Latitud { get; set; }

        [JsonPropertyName("longitud")]
        public double? Longitud { get; set; }
    }
}
