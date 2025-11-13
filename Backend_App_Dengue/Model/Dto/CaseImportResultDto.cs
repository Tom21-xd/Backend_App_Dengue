using Backend_App_Dengue.Data.Entities;

namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO para el resultado de importación masiva de casos
    /// </summary>
    public class CaseImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public List<CaseImportErrorDto> Errors { get; set; } = new List<CaseImportErrorDto>();
        public DateTime ImportedAt { get; set; }
        public int ImportedByUserId { get; set; }
        public TimeSpan ProcessingTime { get; set; }

        /// <summary>
        /// Lista de casos importados exitosamente con sus coordenadas
        /// </summary>
        public List<ImportedCaseDto> ImportedCases { get; set; } = new List<ImportedCaseDto>();
    }

    /// <summary>
    /// DTO para representar un caso importado con sus coordenadas
    /// </summary>
    public class ImportedCaseDto
    {
        public int CaseId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Neighborhood { get; set; }
        public string? TemporaryName { get; set; }
        public int? Year { get; set; }
        public int? Age { get; set; }
        public string DengueType { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para errores individuales durante la importación
    /// </summary>
    public class CaseImportErrorDto
    {
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, string?> RowData { get; set; } = new Dictionary<string, string?>();
    }
}
