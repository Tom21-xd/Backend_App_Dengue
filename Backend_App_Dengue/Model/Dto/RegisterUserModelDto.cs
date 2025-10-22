using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class RegisterUserModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [JsonPropertyName("NOMBRE_USUARIO")]
        public string NOMBRE_USUARIO { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        [JsonPropertyName("CORREO_USUARIO")]
        public string CORREO_USUARIO { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [JsonPropertyName("CONTRASENIA_USUARIO")]
        public string CONTRASENIA_USUARIO { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [JsonPropertyName("DIRECCION_USUARIO")]
        public string DIRECCION_USUARIO { get; set; }

        [JsonPropertyName("FECHA_NACIMIENTO_USUARIO")]
        public string? FECHA_NACIMIENTO_USUARIO { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El rol debe ser válido")]
        [JsonPropertyName("FK_ID_ROL")]
        public int FK_ID_ROL { get; set; }

        [Required(ErrorMessage = "El municipio es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El municipio debe ser válido")]
        [JsonPropertyName("FK_ID_MUNICIPIO")]
        public int FK_ID_MUNICIPIO { get; set; }

        [Required(ErrorMessage = "El tipo de sangre es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El tipo de sangre debe ser válido")]
        [JsonPropertyName("FK_ID_TIPOSANGRE")]
        public int FK_ID_TIPOSANGRE { get; set; }

        [Required(ErrorMessage = "El género es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El género debe ser válido")]
        [JsonPropertyName("FK_ID_GENERO")]
        public int FK_ID_GENERO { get; set; }
    }
}
