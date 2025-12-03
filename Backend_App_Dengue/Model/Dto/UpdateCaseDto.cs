namespace Backend_App_Dengue.Model.Dto
{
    public class UpdateCaseDto
    {
        public int? IdEstadoCaso { get; set; }
        public int? IdTipoDengue { get; set; }
        public string? Descripcion { get; set; }
    }

    /// <summary>
    /// DTO para actualizar solo las coordenadas de un caso
    /// Usado en la pantalla de revisión de importación masiva
    /// </summary>
    public class UpdateCaseCoordinatesDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}

