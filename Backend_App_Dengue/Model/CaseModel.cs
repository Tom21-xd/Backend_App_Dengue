using System.Text.Json.Serialization;
using ThirdParty.Json.LitJson;

namespace Backend_App_Dengue.Model
{
    public class CaseModel
    {
        [JsonPropertyName("ID_CASOREPORTADO")]
        public int ID_CASOREPORTADO { get; set; }
        [JsonPropertyName("DESCRIPCION_CASOREPORTADO")]
        public string DESCRIPCION_CASOREPORTADO { get; set; }
        [JsonPropertyName("FECHA_CASOREPORTADO")]
        public string FECHA_CASOREPORTADO { get; set; }
        [JsonPropertyName("FK_ID_ESTADOCASO")]
        public int FK_ID_ESTADOCASO { get; set; }
        [JsonPropertyName("NOMBRE_ESTADOCASO")]
        public string NOMBRE_ESTADOCASO { get; set; }
        [JsonPropertyName("ID_DEPARTAMENTO")]
        public int ID_DEPARTAMENTO { get; set; }
        [JsonPropertyName("ID_MUNICIPIO")]
        public int ID_MUNICIPIO { get; set; }
        [JsonPropertyName("FK_ID_HOSPITAL")]
        public int FK_ID_HOSPITAL { get; set; }
        [JsonPropertyName("FK_ID_TIPODENGUE")]
        public int FK_ID_TIPODENGUE { get; set; }

        [JsonPropertyName("NOMBRE_TIPODENGUE")]
        public string NOMBRE_TIPODENGUE { get; set; }
        [JsonPropertyName("FK_ID_PACIENTE")]
        public int FK_ID_PACIENTE { get; set; }
        [JsonPropertyName("NOMBRE_PACIENTE")]
        public string NOMBRE_PACIENTE { get; set; }
        [JsonPropertyName("FK_ID_PERSONALMEDICO")]
        public int FK_ID_PERSONALMEDICO { get; set; }
        [JsonPropertyName("NOMBRE_PERSONALMEDICO")]
        public string NOMBRE_PERSONALMEDICO { get; set; }
        [JsonPropertyName("FECHAFINALIZACION_CASO")]
        public string FECHAFINALIZACION_CASO { get; set; }
        [JsonPropertyName("DIRECCION_CASOREPORTADO")]
        public string DIRECCION_CASOREPORTADO { get; set; }
    }
}
