using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class DiagnosticRequestDto
    {
        [JsonPropertyName("sintomas_ids")]
        public List<int> SintomasIds { get; set; }
    }

    public class DiagnosticResponseDto
    {
        [JsonPropertyName("ID_TIPODENGUE")]
        public int IdTipoDengue { get; set; }

        [JsonPropertyName("NOMBRE_TIPODENGUE")]
        public string NombreTipoDengue { get; set; }

        [JsonPropertyName("puntaje")]
        public int Puntaje { get; set; }

        [JsonPropertyName("sintomas_coincidentes")]
        public int SintomasCoincidentes { get; set; }

        [JsonPropertyName("total_sintomas")]
        public int TotalSintomas { get; set; }

        [JsonPropertyName("porcentaje_coincidencia")]
        public decimal PorcentajeCoincidencia { get; set; }

        [JsonPropertyName("diagnostico")]
        public string Diagnostico { get; set; }
    }
}
