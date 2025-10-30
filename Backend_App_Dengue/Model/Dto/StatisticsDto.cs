using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    // Estadísticas Generales
    public class GeneralStatsModel
    {
        [JsonPropertyName("total_casos")]
        public int TotalCasos { get; set; }

        [JsonPropertyName("casos_activos")]
        public int CasosActivos { get; set; }

        [JsonPropertyName("casos_finalizados")]
        public int CasosFinalizados { get; set; }

        [JsonPropertyName("casos_fallecidos")]
        public int CasosFallecidos { get; set; }

        [JsonPropertyName("usuarios_enfermos")]
        public int UsuariosEnfermos { get; set; }

        [JsonPropertyName("hospitales_activos")]
        public int HospitalesActivos { get; set; }

        [JsonPropertyName("total_publicaciones")]
        public int TotalPublicaciones { get; set; }

        [JsonPropertyName("notificaciones_pendientes")]
        public int NotificacionesPendientes { get; set; }
    }

    // Estadísticas por Tipo de Dengue
    public class DengueTypeStatsModel
    {
        [JsonPropertyName("ID_TIPODENGUE")]
        public int IdTipoDengue { get; set; }

        [JsonPropertyName("NOMBRE_TIPODENGUE")]
        public string NombreTipoDengue { get; set; }

        [JsonPropertyName("total_casos")]
        public int TotalCasos { get; set; }

        [JsonPropertyName("casos_activos")]
        public int CasosActivos { get; set; }

        [JsonPropertyName("casos_finalizados")]
        public int CasosFinalizados { get; set; }

        [JsonPropertyName("casos_fallecidos")]
        public int CasosFallecidos { get; set; }

        [JsonPropertyName("tasa_mortalidad")]
        public decimal TasaMortalidad { get; set; }
    }

    // Casos por Mes
    public class MonthlyStatsModel
    {
        [JsonPropertyName("mes")]
        public int Mes { get; set; }

        [JsonPropertyName("nombre_mes")]
        public string NombreMes { get; set; }

        [JsonPropertyName("total_casos")]
        public int TotalCasos { get; set; }

        [JsonPropertyName("sin_alarma")]
        public int SinAlarma { get; set; }

        [JsonPropertyName("con_alarma")]
        public int ConAlarma { get; set; }

        [JsonPropertyName("grave")]
        public int Grave { get; set; }
    }

    // Casos por Departamento
    public class DepartmentStatsModel
    {
        [JsonPropertyName("ID_DEPARTAMENTO")]
        public int IdDepartamento { get; set; }

        [JsonPropertyName("NOMBRE_DEPARTAMENTO")]
        public string NombreDepartamento { get; set; }

        [JsonPropertyName("total_casos")]
        public int TotalCasos { get; set; }

        [JsonPropertyName("casos_activos")]
        public int CasosActivos { get; set; }

        [JsonPropertyName("hospitales_involucrados")]
        public int HospitalesInvolucrados { get; set; }
    }

    // Tendencia de Casos
    public class TrendStatsModel
    {
        [JsonPropertyName("periodo")]
        public string Periodo { get; set; }

        [JsonPropertyName("total_casos")]
        public int TotalCasos { get; set; }

        [JsonPropertyName("tasa_mortalidad")]
        public decimal TasaMortalidad { get; set; }
    }

    // Top Hospitales
    public class TopHospitalModel
    {
        [JsonPropertyName("ID_HOSPITAL")]
        public int? IdHospital { get; set; }

        [JsonPropertyName("NOMBRE_HOSPITAL")]
        public string NombreHospital { get; set; }

        [JsonPropertyName("NOMBRE_MUNICIPIO")]
        public string NombreMunicipio { get; set; }

        [JsonPropertyName("NOMBRE_DEPARTAMENTO")]
        public string NombreDepartamento { get; set; }

        [JsonPropertyName("total_casos")]
        public int TotalCasos { get; set; }

        [JsonPropertyName("casos_activos")]
        public int CasosActivos { get; set; }
    }

    // Casos para Mapa
    public class MapCaseModel
    {
        [JsonPropertyName("ID_CASOREPORTADO")]
        public int IdCaso { get; set; }

        [JsonPropertyName("LATITUD")]
        public decimal Latitud { get; set; }

        [JsonPropertyName("LONGITUD")]
        public decimal Longitud { get; set; }

        [JsonPropertyName("DESCRIPCION_CASO")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("DIRECCION_CASO")]
        public string? Direccion { get; set; }

        [JsonPropertyName("BARRIO_VEREDA")]
        public string? Barrio { get; set; }

        [JsonPropertyName("NOMBRE_PACIENTE")]
        public string NombrePaciente { get; set; }

        [JsonPropertyName("NOMBRE_HOSPITAL")]
        public string NombreHospital { get; set; }

        [JsonPropertyName("TIPO_DENGUE")]
        public string TipoDengue { get; set; }

        [JsonPropertyName("FK_ID_TIPODENGUE")]
        public int IdTipoDengue { get; set; }

        [JsonPropertyName("ESTADO")]
        public string Estado { get; set; }

        [JsonPropertyName("FK_ID_ESTADO")]
        public int IdEstado { get; set; }

        [JsonPropertyName("FECHA_REGISTRO")]
        public DateTime FechaRegistro { get; set; }

        [JsonPropertyName("ANIO_REPORTE")]
        public int? AnioReporte { get; set; }

        [JsonPropertyName("EDAD_PACIENTE")]
        public int? EdadPaciente { get; set; }
    }
}
