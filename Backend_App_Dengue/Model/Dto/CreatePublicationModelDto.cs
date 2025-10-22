namespace Backend_App_Dengue.Model.Dto
{
    public class CreatePublicationModelDto
    {

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public IFormFile imagen { get; set; }

        public string UsuarioId { get; set; }

        // Nueva: Categoría de la publicación (obligatoria)
        public int? CategoriaId { get; set; }

        // Nuevas: Etiquetas de la publicación (opcional, múltiples)
        // Formato: "1,2,3,4" (IDs separados por coma)
        public string? EtiquetasIds { get; set; }

        // Opcionales: Campos avanzados
        public string? Prioridad { get; set; } // "Baja", "Normal", "Alta", "Urgente"
        public bool Fijada { get; set; } = false;

        // Geolocalización automática
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
    }
}
