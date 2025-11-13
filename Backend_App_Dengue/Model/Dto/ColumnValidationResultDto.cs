namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// Resultado de la validación de columnas de un archivo de importación
    /// </summary>
    public class ColumnValidationResultDto
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> DetectedColumns { get; set; } = new List<string>();
        public Dictionary<string, string?> SuggestedMapping { get; set; } = new Dictionary<string, string?>();
        public List<string> RequiredFields { get; set; } = new List<string>
        {
            "año",
            "edad",
            "clasificacion",
            "barrio"
        };
        public List<string> OptionalFields { get; set; } = new List<string>
        {
            "latitud",
            "longitud",
            "ciudad",
            "direccion",
            "nombre"
        };
    }
}
