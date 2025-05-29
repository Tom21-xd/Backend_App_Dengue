using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class UserModel
    {
        [JsonPropertyName("ID_USUARIO")]
        public int ID_USUARIO { get; set; }
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
        [JsonPropertyName("NOMBRE_ROL")]
        public string? NOMBRE_ROL { get; set; }
        [JsonPropertyName("FK_ID_MUNICIPIO")]
        public int FK_ID_MUNICIPIO { get; set; }
        [JsonPropertyName("NOMBRE_MUNICIPIO")]
        public string? NOMBRE_MUNICIPIO { get; set; }
        [JsonPropertyName("FK_ID_TIPOSANGRE")]
        public int FK_ID_TIPOSANGRE { get; set; }
        [JsonPropertyName("NOMBRE_TIPOSANGRE")]
        public string? NOMBRE_TIPOSANGRE { get; set; }
        [JsonPropertyName("FK_ID_GENERO")]
        public int FK_ID_GENERO { get; set; }
        [JsonPropertyName("NOMBRE_GENERO")]
        public string? NOMBRE_GENERO { get; set; }
        [JsonPropertyName("FK_ID_DEPARTAMENTO")]
        public int? ID_DEPARTAMENTO { get; set; }
        [JsonPropertyName("NOMBRE_ESTADOUSUARIO")]
        public string? NOMBRE_ESTADOUSUARIO { get; set; }

    }
}
