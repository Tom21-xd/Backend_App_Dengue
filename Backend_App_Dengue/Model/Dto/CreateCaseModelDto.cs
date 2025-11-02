using System.ComponentModel.DataAnnotations;

namespace Backend_App_Dengue.Model.Dto
{
    public class CreateCaseModelDto
    {
        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string descripcion { get; set; } = string.Empty;

        public int? id_hospital { get; set; }

        [Required(ErrorMessage = "El tipo de dengue es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El tipo de dengue debe ser válido")]
        public int id_tipoDengue { get; set; }

        public int? id_paciente { get; set; }

        public int? id_personalMedico { get; set; }

        [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres")]
        public string? direccion { get; set; }

        [Range(1900, 2100, ErrorMessage = "El año debe estar entre 1900 y 2100")]
        public int? anio_reporte { get; set; }

        [Range(0, 130, ErrorMessage = "La edad debe estar entre 0 y 130")]
        public int? edad { get; set; }

        [StringLength(200, ErrorMessage = "El nombre temporal no puede exceder 200 caracteres")]
        public string? nombre_temporal { get; set; }

        [StringLength(255, ErrorMessage = "El barrio o vereda no puede exceder 255 caracteres")]
        public string? barrio { get; set; }

        public decimal? latitud { get; set; }

        public decimal? longitud { get; set; }
    }
}
