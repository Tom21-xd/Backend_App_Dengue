using CsvHelper.Configuration.Attributes;

namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO para mapear filas del CSV/Excel de importación de casos
    /// </summary>
    public class CaseImportDto
    {
        [Name("año_")]
        public string? Year { get; set; }

        [Name("edad_")]
        public string? Age { get; set; }

        [Name("clasificacion del caso")]
        public string? Classification { get; set; }

        [Name("sNA")]
        public string? SNA { get; set; }

        [Name("bar_ver_")]
        public string? Neighborhood { get; set; }

        [Name("latitud (ISO)")]
        public string? Latitude { get; set; }

        [Name("longitud (ISO)")]
        public string? Longitude { get; set; }

        [Name("exo_")]
        public string? Exo { get; set; }

        [Name("COMU")]
        public string? Community { get; set; }
    }
}
