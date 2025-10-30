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
