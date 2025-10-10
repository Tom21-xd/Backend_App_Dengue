using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        internal Connection cn = new Connection();

        // HU-013: Inferir Tipo de Dengue
        [HttpPost]
        [Route("diagnoseDengue")]
        [ProducesResponseType(typeof(List<DiagnosticResponseDto>), 200)]
        [ProducesResponseType(400)]
        public IActionResult DiagnoseDengue([FromBody] DiagnosticRequestDto request)
        {
            if (request == null || request.SintomasIds == null || request.SintomasIds.Count == 0)
            {
                return BadRequest(new { message = "Debe proporcionar al menos un síntoma para el diagnóstico" });
            }

            try
            {
                // Convertir lista de IDs a string separado por comas
                string sintomasString = string.Join(",", request.SintomasIds);

                string[] parametros = { "sintomas_ids" };
                string[] valores = { sintomasString };

                DataTable dt = cn.ProcedimientosSelect(parametros, "InferirTipoDengue", valores);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Ok(new
                    {
                        message = "No se pudo determinar el tipo de dengue con los síntomas proporcionados",
                        resultados = new List<DiagnosticResponseDto>()
                    });
                }

                // Convertir DataTable a lista de resultados
                var resultados = new List<DiagnosticResponseDto>();

                foreach (DataRow row in dt.Rows)
                {
                    int idTipoDengue = Convert.ToInt32(row["ID_TIPODENGUE"]);
                    string nombreTipoDengue = row["NOMBRE_TIPODENGUE"].ToString() ?? "";
                    int puntaje = Convert.ToInt32(row["puntaje"]);
                    int sintomasCoincidentes = Convert.ToInt32(row["sintomas_coincidentes"]);
                    int totalSintomas = Convert.ToInt32(row["total_sintomas"]);
                    decimal porcentajeCoincidencia = Convert.ToDecimal(row["porcentaje_coincidencia"]);

                    string diagnostico = "";
                    if (porcentajeCoincidencia >= 80)
                    {
                        diagnostico = "Alta probabilidad";
                    }
                    else if (porcentajeCoincidencia >= 50)
                    {
                        diagnostico = "Probabilidad moderada";
                    }
                    else if (porcentajeCoincidencia >= 30)
                    {
                        diagnostico = "Baja probabilidad";
                    }
                    else
                    {
                        diagnostico = "Probabilidad muy baja";
                    }

                    resultados.Add(new DiagnosticResponseDto
                    {
                        IdTipoDengue = idTipoDengue,
                        NombreTipoDengue = nombreTipoDengue,
                        Puntaje = puntaje,
                        SintomasCoincidentes = sintomasCoincidentes,
                        TotalSintomas = totalSintomas,
                        PorcentajeCoincidencia = porcentajeCoincidencia,
                        Diagnostico = diagnostico
                    });
                }

                // Ordenar por puntaje descendente
                resultados = resultados.OrderByDescending(r => r.Puntaje).ToList();

                return Ok(new
                {
                    message = "Diagnóstico realizado con éxito",
                    sintomas_evaluados = request.SintomasIds.Count,
                    resultados = resultados,
                    recomendacion = resultados.Count > 0 && resultados[0].PorcentajeCoincidencia >= 50
                        ? "Se recomienda acudir a un centro médico para confirmación del diagnóstico"
                        : "Si presenta síntomas persistentes, consulte a un profesional de la salud"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al realizar el diagnóstico", error = ex.Message });
            }
        }

        // Endpoint adicional: Obtener lista de síntomas disponibles
        [HttpGet]
        [Route("getSymptoms")]
        [ProducesResponseType(200)]
        public IActionResult GetSymptoms()
        {
            try
            {
                DataTable dt = cn.ProcedimientosSelect(null, "ListarSintomas", null);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Ok(new List<object>());
                }

                var sintomas = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    sintomas.Add(new
                    {
                        id = Convert.ToInt32(row["ID_SINTOMA"]),
                        nombre = row["NOMBRE_SINTOMA"].ToString(),
                        descripcion = row.Table.Columns.Contains("DESCRIPCION_SINTOMA")
                            ? row["DESCRIPCION_SINTOMA"].ToString()
                            : ""
                    });
                }

                return Ok(sintomas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los síntomas", error = ex.Message });
            }
        }
    }
}
