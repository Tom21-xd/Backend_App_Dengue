using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        internal Connection cn = new Connection();

        // HU-008: Estadísticas Generales
        [HttpGet]
        [Route("general")]
        [ProducesResponseType(typeof(GeneralStatsModel), 200)]
        public IActionResult GetGeneralStatistics()
        {
            try
            {
                DataTable dt = cn.ProcedimientosSelect(null, "EstadisticasGenerales", null);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Ok(new GeneralStatsModel());
                }

                var row = dt.Rows[0];
                var stats = new GeneralStatsModel
                {
                    TotalCasos = row.Table.Columns.Contains("total_casos") ? Convert.ToInt32(row["total_casos"]) : 0,
                    CasosActivos = row.Table.Columns.Contains("casos_activos") ? Convert.ToInt32(row["casos_activos"]) : 0,
                    CasosFinalizados = row.Table.Columns.Contains("casos_recuperados") ? Convert.ToInt32(row["casos_recuperados"]) : 0,
                    CasosFallecidos = row.Table.Columns.Contains("fallecidos") ? Convert.ToInt32(row["fallecidos"]) : 0,
                    UsuariosEnfermos = row.Table.Columns.Contains("usuarios_enfermos") ? Convert.ToInt32(row["usuarios_enfermos"]) : 0,
                    HospitalesActivos = row.Table.Columns.Contains("hospitales_activos") ? Convert.ToInt32(row["hospitales_activos"]) : 0,
                    TotalPublicaciones = row.Table.Columns.Contains("total_publicaciones") ? Convert.ToInt32(row["total_publicaciones"]) : 0,
                    NotificacionesPendientes = 0 // Se puede agregar después si es necesario
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las estadísticas generales", error = ex.Message });
            }
        }

        // HU-008: Casos por Tipo de Dengue
        [HttpGet]
        [Route("byDengueType")]
        [ProducesResponseType(typeof(List<DengueTypeStatsModel>), 200)]
        public IActionResult GetCasesByDengueType()
        {
            try
            {
                DataTable dt = cn.ProcedimientosSelect(null, "CasosPorTipoDengue", null);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Ok(new List<DengueTypeStatsModel>());
                }

                var stats = new List<DengueTypeStatsModel>();
                foreach (DataRow row in dt.Rows)
                {
                    stats.Add(new DengueTypeStatsModel
                    {
                        IdTipoDengue = Convert.ToInt32(row["ID_TIPODENGUE"]),
                        NombreTipoDengue = row["NOMBRE_TIPODENGUE"].ToString() ?? "",
                        TotalCasos = Convert.ToInt32(row["total_casos"]),
                        CasosActivos = row.Table.Columns.Contains("casos_activos") ? Convert.ToInt32(row["casos_activos"]) : 0,
                        CasosFinalizados = row.Table.Columns.Contains("casos_recuperados") ? Convert.ToInt32(row["casos_recuperados"]) : 0,
                        CasosFallecidos = row.Table.Columns.Contains("fallecidos") ? Convert.ToInt32(row["fallecidos"]) : 0,
                        TasaMortalidad = row.Table.Columns.Contains("tasa_mortalidad")
                            ? Convert.ToDecimal(row["tasa_mortalidad"])
                            : (row.Table.Columns.Contains("porcentaje_del_total") ? Convert.ToDecimal(row["porcentaje_del_total"]) : 0)
                    });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas por tipo de dengue", error = ex.Message });
            }
        }

        // HU-008: Casos por Mes
        [HttpGet]
        [Route("byMonth")]
        [ProducesResponseType(typeof(List<MonthlyStatsModel>), 200)]
        public IActionResult GetCasesByMonth([FromQuery] int? year)
        {
            try
            {
                string[] parametros = { "anio" };
                string[] valores = { (year ?? DateTime.Now.Year).ToString() };

                DataTable dt = cn.ProcedimientosSelect(parametros, "CasosPorMes", valores);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Ok(new List<MonthlyStatsModel>());
                }

                var stats = new List<MonthlyStatsModel>();
                foreach (DataRow row in dt.Rows)
                {
                    stats.Add(new MonthlyStatsModel
                    {
                        Mes = Convert.ToInt32(row["mes"]),
                        NombreMes = row["nombre_mes"].ToString() ?? "",
                        TotalCasos = Convert.ToInt32(row["total_casos"]),
                        SinAlarma = row.Table.Columns.Contains("sin_alarma") ? Convert.ToInt32(row["sin_alarma"]) : 0,
                        ConAlarma = row.Table.Columns.Contains("con_alarma") ? Convert.ToInt32(row["con_alarma"]) : 0,
                        Grave = row.Table.Columns.Contains("grave") ? Convert.ToInt32(row["grave"]) : 0
                    });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas mensuales", error = ex.Message });
            }
        }

        // HU-008: Casos por Departamento
        [HttpGet]
        [Route("byDepartment")]
        [ProducesResponseType(typeof(List<DepartmentStatsModel>), 200)]
        public IActionResult GetCasesByDepartment()
        {
            try
            {
                DataTable dt = cn.ProcedimientosSelect(null, "CasosPorDepartamento", null);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Ok(new List<DepartmentStatsModel>());
                }

                var stats = new List<DepartmentStatsModel>();
                foreach (DataRow row in dt.Rows)
                {
                    stats.Add(new DepartmentStatsModel
                    {
                        IdDepartamento = Convert.ToInt32(row["ID_DEPARTAMENTO"]),
                        NombreDepartamento = row["NOMBRE_DEPARTAMENTO"].ToString() ?? "",
                        TotalCasos = Convert.ToInt32(row["total_casos"]),
                        CasosActivos = row.Table.Columns.Contains("casos_activos") ? Convert.ToInt32(row["casos_activos"]) : 0,
                        HospitalesInvolucrados = row.Table.Columns.Contains("hospitales_involucrados")
                            ? Convert.ToInt32(row["hospitales_involucrados"])
                            : 0
                    });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas por departamento", error = ex.Message });
            }
        }

        // HU-008: Tendencia de Casos
        [HttpGet]
        [Route("trends")]
        [ProducesResponseType(typeof(List<TrendStatsModel>), 200)]
        public IActionResult GetCaseTrends([FromQuery] int? months)
        {
            try
            {
                string[] parametros = { "meses" };
                string[] valores = { (months ?? 6).ToString() };

                DataTable dt = cn.ProcedimientosSelect(parametros, "TendenciaCasos", valores);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Ok(new List<TrendStatsModel>());
                }

                var stats = new List<TrendStatsModel>();
                foreach (DataRow row in dt.Rows)
                {
                    stats.Add(new TrendStatsModel
                    {
                        Periodo = row["periodo"].ToString() ?? "",
                        TotalCasos = Convert.ToInt32(row["total_casos"]),
                        TasaMortalidad = row.Table.Columns.Contains("tasa_mortalidad") && row["tasa_mortalidad"] != DBNull.Value
                            ? Convert.ToDecimal(row["tasa_mortalidad"])
                            : 0
                    });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener tendencias de casos", error = ex.Message });
            }
        }

        // HU-008: Top Hospitales por Casos
        [HttpGet]
        [Route("topHospitals")]
        [ProducesResponseType(typeof(List<TopHospitalModel>), 200)]
        public IActionResult GetTopHospitals([FromQuery] int? limit)
        {
            try
            {
                string[] parametros = { "limite" };
                string[] valores = { (limit ?? 10).ToString() };

                DataTable dt = cn.ProcedimientosSelect(parametros, "TopHospitalesCasos", valores);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Ok(new List<TopHospitalModel>());
                }

                var stats = new List<TopHospitalModel>();
                foreach (DataRow row in dt.Rows)
                {
                    stats.Add(new TopHospitalModel
                    {
                        IdHospital = Convert.ToInt32(row["ID_HOSPITAL"]),
                        NombreHospital = row["NOMBRE_HOSPITAL"].ToString() ?? "",
                        NombreMunicipio = row.Table.Columns.Contains("municipio")
                            ? row["municipio"].ToString() ?? ""
                            : (row.Table.Columns.Contains("NOMBRE_MUNICIPIO") ? row["NOMBRE_MUNICIPIO"].ToString() ?? "" : ""),
                        NombreDepartamento = row.Table.Columns.Contains("departamento")
                            ? row["departamento"].ToString() ?? ""
                            : (row.Table.Columns.Contains("NOMBRE_DEPARTAMENTO") ? row["NOMBRE_DEPARTAMENTO"].ToString() ?? "" : ""),
                        TotalCasos = Convert.ToInt32(row["total_casos"]),
                        CasosActivos = row.Table.Columns.Contains("casos_activos") ? Convert.ToInt32(row["casos_activos"]) : 0
                    });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener top de hospitales", error = ex.Message });
            }
        }
    }
}
