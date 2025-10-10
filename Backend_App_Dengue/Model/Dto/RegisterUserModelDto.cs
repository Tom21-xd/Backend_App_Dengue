using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class RegisterUserModel
    {
        [JsonPropertyName("NOMBRE_USUARIO")]
        public string NOMBRE_USUARIO { get; set; }
        [JsonPropertyName("CORREO_USUARIO")]
        public string CORREO_USUARIO { get; set; }
        [JsonPropertyName("CONTRASENIA_USUARIO")]
        public string CONTRASENIA_USUARIO { get; set; }
        [JsonPropertyName("DIRECCION_USUARIO")]
        public string DIRECCION_USUARIO { get; set; }
        [JsonPropertyName("FK_ID_ROL")]
        public int FK_ID_ROL { get; set; }
        [JsonPropertyName("FK_ID_MUNICIPIO")]
        public int FK_ID_MUNICIPIO { get; set; }
        [JsonPropertyName("FK_ID_TIPOSANGRE")]
        public int FK_ID_TIPOSANGRE { get; set; }
        [JsonPropertyName("FK_ID_GENERO")]
        public int FK_ID_GENERO { get; set; }

    }
}
