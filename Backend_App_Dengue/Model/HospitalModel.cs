using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class HospitalModel
    {
        [JsonPropertyName("ID_HOSPITAL")]
        public int ID_HOSPITAL { get; set; }
        [JsonPropertyName("NOMBRE_HOSPITAL")]
        public string NOMBRE_HOSPITAL { get; set; }
        [JsonPropertyName("ESTADO_HOSPITAL")]
        public int ESTADO_HOSPITAL { get; set; }
        [JsonPropertyName("DIRECCION_HOSPITAL")]
        public string DIRECCION_HOSPITAL { get; set; }
        [JsonPropertyName("LATITUD_HOSPITAL")]
        public string LATITUD_HOSPITAL { get; set; }
        [JsonPropertyName("LONGITUD_HOSPITAL")]
        public string LONGITUD_HOSPITAL { get; set; }
        [JsonPropertyName("FK_ID_MUNICIPIO")]
        public int FK_ID_MUNICIPIO { get; set; }
        [JsonPropertyName("IMAGEN_HOSPITAL")]
        public string IMAGEN_HOSPITAL { get; set; }
        //public IFormFile? File { get; set; }
        //public ImagenModel Imagen { get; set; }
        [JsonPropertyName("CANTIDADCASOS_HOSPITAL")]
        public int CANTIDADCASOS_HOSPITAL { get; set; }
        [JsonPropertyName("FK_ID_DEPARTAMENTO")]
        public int FK_ID_DEPARTAMENTO { get; set; }
        [JsonPropertyName("NOMBRE_DEPARTAMENTO")]
        public string NOMBRE_DEPARTAMENTO { get; set; }
    }
}
