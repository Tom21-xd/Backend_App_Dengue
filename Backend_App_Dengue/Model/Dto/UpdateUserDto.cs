using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class UpdateUserDto
    {
        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("correo")]
        public string? Correo { get; set; }

        [JsonPropertyName("direccion")]
        public string? Direccion { get; set; }

        [JsonPropertyName("id_rol")]
        public int? IdRol { get; set; }

        [JsonPropertyName("id_municipio")]
        public int? IdMunicipio { get; set; }

        [JsonPropertyName("id_genero")]
        public int? IdGenero { get; set; }
    }
}
