using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
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
        public IActionResult createCase([FromBody] CreateCaseModelDto caso)
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
        public IActionResult UpdateCase(int id, [FromBody] UpdateCaseDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos de actualización son requeridos" });
            }

            try
            {
                string[] parametros = { "ID_CASO", "ID_ESTADOCASO", "ID_TIPODENGUE", "DESCRIPCION" };
                string[] valores = {
                    id.ToString(),
                    dto.IdEstadoCaso?.ToString() ?? "NULL",
                    dto.IdTipoDengue?.ToString() ?? "NULL",
                    dto.Descripcion ?? "NULL"
                };

                cn.procedimientosInEd(parametros, "EditarCaso", valores);

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
