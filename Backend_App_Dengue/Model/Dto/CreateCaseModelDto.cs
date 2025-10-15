using System.ComponentModel.DataAnnotations;

namespace Backend_App_Dengue.Model.Dto
{
    public class CreateCaseModelDto
    {
        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string descripcion { get; set; }

        [Required(ErrorMessage = "El hospital es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El hospital debe ser válido")]
        public int id_hospital { get; set; }

        [Required(ErrorMessage = "El tipo de dengue es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El tipo de dengue debe ser válido")]
        public int id_tipoDengue { get; set; }

        [Required(ErrorMessage = "El paciente es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El paciente debe ser válido")]
        public int id_paciente { get; set; }

        [Required(ErrorMessage = "El personal médico es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El personal médico debe ser válido")]
        public int id_personalMedico { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres")]
        public string direccion { get; set; }
    }
}
