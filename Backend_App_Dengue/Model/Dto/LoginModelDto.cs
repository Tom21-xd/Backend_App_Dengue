using System.ComponentModel.DataAnnotations;

namespace Backend_App_Dengue.Model.Dto
{
    public class LoginModelDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string? email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string? password { get; set; }
    }
}
