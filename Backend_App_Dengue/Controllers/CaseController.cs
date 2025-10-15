using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Backend_App_Dengue.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CaseController : ControllerBase
    {
        internal Connection cn = new Connection();
        private readonly FCMService _fcmService = new FCMService();

        [HttpGet]
        [Route("getCases")]
        public IActionResult getCases()
        {
            try
            {
                DataTable tb = cn.ProcedimientosSelect(null, "ListarCasos", null);
                List<CaseModel> casos = tb.DataTableToList<CaseModel>();

                if (casos == null)
                {
                    return Ok(new List<CaseModel>());
                }

                return Ok(casos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los casos", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("createCase")]
        public async Task<IActionResult> createCase([FromBody] CreateCaseModelDto caso)
        {
            if (caso == null)
            {
                return BadRequest(new { message = "Los datos del caso son requeridos" });
            }

            try
            {
                string[] parametros = { "descri", "ihospital", "tdengue", "paciente", "personalmedico", "direccion" };
                string[] aux = {
                    caso.descripcion,
                    caso.id_hospital.ToString(),
                    caso.id_tipoDengue.ToString(),
                    caso.id_paciente.ToString(),
                    caso.id_personalMedico.ToString(),
                    caso.direccion
                };
                cn.procedimientosInEd(parametros, "CrearCaso", aux);

                // Enviar notificación push al personal médico (rol 3)
                try
                {
                    DataTable dtTokens = cn.ProcedimientosSelect(new string[] { "rolId" }, "ObtenerFCMTokensPorRol", new string[] { "3" });
                    List<string> tokens = new List<string>();

                    foreach (DataRow row in dtTokens.Rows)
                    {
                        tokens.Add(row["FCM_TOKEN"].ToString());
                    }

                    if (tokens.Count > 0)
                    {
                        var data = new Dictionary<string, string>
                        {
                            { "type", "new_case" },
                            { "caso_id", caso.id_hospital.ToString() }
                        };

                        await _fcmService.SendNotificationToMultipleDevices(
                            tokens,
                            "Nuevo Caso de Dengue Reportado",
                            $"Se ha reportado un nuevo caso de dengue. Revisa los detalles en la aplicación.",
                            data
                        );
                    }
                }
                catch (Exception fcmEx)
                {
                    Console.WriteLine($"Error al enviar notificación push: {fcmEx.Message}");
                    // No fallar la creación del caso si falla la notificación
                }

                return Ok(new { message = "Se ha creado el caso con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el caso", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("getCaseById")]
        public IActionResult getCaseById([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "El ID del caso es requerido" });
            }

            try
            {
                string[] parametros = { "idc" };
                string[] aux = { id };
                DataTable tb = cn.ProcedimientosSelect(parametros, "ObtenerCaso", aux);
                List<CaseModel> casos = tb.DataTableToList<CaseModel>();

                if (casos == null || casos.Count == 0)
                {
                    return NotFound(new { message = "No se ha encontrado el caso" });
                }

                return Ok(casos[0]);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el caso", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("getStateCase")]
        public IActionResult getStateCase()
        {
            try
            {
                DataTable tb = cn.ProcedimientosSelect(null, "ListarEstadocaso", null);
                List<CaseStatesModel> casos = tb.DataTableToList<CaseStatesModel>();

                if (casos == null || casos.Count == 0)
                {
                    return Ok(new List<CaseStatesModel>());
                }

                return Ok(casos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los estados de caso", error = ex.Message });
            }
        }

        [HttpPatch]
        [Route("updateCase/{id}")]
        public async Task<IActionResult> UpdateCase(int id, [FromBody] UpdateCaseDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos de actualización son requeridos" });
            }

            try
            {
                // Obtener información del caso antes de actualizar
                string[] getCaseParams = { "idc" };
                string[] getCaseValues = { id.ToString() };
                DataTable dtCaso = cn.ProcedimientosSelect(getCaseParams, "ObtenerCaso", getCaseValues);

                string[] parametros = { "ID_CASO", "ID_ESTADOCASO", "ID_TIPODENGUE", "DESCRIPCION" };
                string[] valores = {
                    id.ToString(),
                    dto.IdEstadoCaso?.ToString() ?? "NULL",
                    dto.IdTipoDengue?.ToString() ?? "NULL",
                    dto.Descripcion ?? "NULL"
                };

                cn.procedimientosInEd(parametros, "EditarCaso", valores);

                // Enviar notificaciones push
                try
                {
                    // Obtener tokens del personal médico
                    DataTable dtTokens = cn.ProcedimientosSelect(new string[] { "rolId" }, "ObtenerFCMTokensPorRol", new string[] { "3" });
                    List<string> tokens = new List<string>();

                    foreach (DataRow row in dtTokens.Rows)
                    {
                        tokens.Add(row["FCM_TOKEN"].ToString());
                    }

                    if (tokens.Count > 0)
                    {
                        // Verificar si el caso fue finalizado (estado 3 = Finalizado, ajusta según tu BD)
                        bool casoFinalizado = dto.IdEstadoCaso.HasValue && dto.IdEstadoCaso.Value == 3;

                        var data = new Dictionary<string, string>
                        {
                            { "type", casoFinalizado ? "case_finished" : "case_updated" },
                            { "caso_id", id.ToString() }
                        };

                        string titulo = casoFinalizado ? "Caso Finalizado" : "Caso Actualizado";
                        string mensaje = casoFinalizado
                            ? $"El caso #{id} ha sido finalizado."
                            : $"El caso #{id} ha sido actualizado. Revisa los cambios.";

                        await _fcmService.SendNotificationToMultipleDevices(
                            tokens,
                            titulo,
                            mensaje,
                            data
                        );
                    }
                }
                catch (Exception fcmEx)
                {
                    Console.WriteLine($"Error al enviar notificación push: {fcmEx.Message}");
                    // No fallar la actualización si falla la notificación
                }

                return Ok(new { message = "Caso actualizado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el caso", error = ex.Message });
            }
        }

        // HU-006: Eliminar Caso
        [HttpDelete]
        [Route("deleteCase/{id}")]
        public IActionResult DeleteCase(int id)
        {
            try
            {
                string[] parametros = { "idc" };
                string[] valores = { id.ToString() };

                cn.procedimientosInEd(parametros, "EliminarCaso", valores);

                return Ok(new { message = "Caso eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el caso", error = ex.Message });
            }
        }

        // HU-012: Historial de Casos de un Paciente
        [HttpGet]
        [Route("getCaseHistory/{userId}")]
        public IActionResult GetCaseHistory(int userId)
        {
            try
            {
                string[] parametros = { "idp" };
                string[] valores = { userId.ToString() };

                DataTable tb = cn.ProcedimientosSelect(parametros, "HistorialCasos", valores);
                List<CaseModel> casos = tb.DataTableToList<CaseModel>();

                if (casos == null)
                {
                    return Ok(new List<CaseModel>());
                }

                return Ok(casos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el historial de casos", error = ex.Message });
            }
        }

        // Obtener Casos por Hospital
        [HttpGet]
        [Route("getCasesByHospital/{hospitalId}")]
        public IActionResult GetCasesByHospital(int hospitalId)
        {
            try
            {
                string[] parametros = { "idh" };
                string[] valores = { hospitalId.ToString() };

                DataTable tb = cn.ProcedimientosSelect(parametros, "CasosXHospital", valores);
                List<CaseModel> casos = tb.DataTableToList<CaseModel>();

                if (casos == null)
                {
                    return Ok(new List<CaseModel>());
                }

                return Ok(casos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los casos del hospital", error = ex.Message });
            }
        }
    }
}
