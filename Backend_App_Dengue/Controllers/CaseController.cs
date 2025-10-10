using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;
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
        public async Task<IActionResult> getCases()
        {
            DataTable tb = cn.ProcedimientosSelect(null, "ListarCasos", null);
            List<CaseModel> casos = tb.DataTableToList<CaseModel>();
            return Ok(casos);
        }

        [HttpPost]
        [Route("creatreCase")]
        public async Task<IActionResult> creatreCase(CreateCaseModelDto caso)
        {
            string[] parametros = { "descri", "ihospital", "tdengue", "paciente", "personalmedico", "direccion" };
            string[] aux = { caso.descripcion, caso.id_hospital + "", caso.id_tipoDengue + "", caso.id_paciente + "", caso.id_personalMedico + "", caso.direccion.ToString() };
            cn.procedimientosInEd(parametros, "CrearCaso", aux);
            return Ok(new { message = "Se ha creado el caso con exito" });
        }

        [HttpGet]
        [Route("getCaseById")]
        public async Task<IActionResult> getCaseById([FromQuery] string id)
        {
            string[] parametros = { "idc" };
            string[] aux = { id + "" };
            DataTable tb = cn.ProcedimientosSelect(parametros, "ObtenerCaso", aux);
            List<CaseModel> casos = tb.DataTableToList<CaseModel>();
            if (casos.Count == 0)
            {
                return NotFound(new { message = "No se ha encontrado el caso" });
            }
            else
            {
                return Ok(casos[0]);
            }
        }

        [HttpGet]
        [Route("getStateCase")]
        public async Task<IActionResult> getStateCase()
        {

            DataTable tb = cn.ProcedimientosSelect(null, "ListarEstadocaso", null);
            List<CaseStatesModel> casos = tb.DataTableToList<CaseStatesModel>();
            if (casos.Count == 0)
            {
                return NotFound(new { message = "No se ha encontrado el caso" });
            }
            else
            {
                return Ok(casos);
            }
        }

        [HttpPatch]
        [Route("updateCase/{id}")]
        public async Task<IActionResult> UpdateCase(int id, [FromBody] UpdateCaseDto dto)
        {
            try
            {
                string[] parametros = { "ID_CASO", "ID_ESTADOCASO", "ID_TIPODENGUE", "DESCRIPCION" };
                string[] valores = {
            id.ToString(),
            dto.IdEstadoCaso?.ToString() ?? null,
            dto.IdTipoDengue?.ToString() ?? null,
            dto.Descripcion ?? null
        };

                cn.procedimientosInEd(parametros, "EditarCaso", valores);

                return Ok(new { message = "Caso actualizado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el caso", error = ex.Message });
            }
        }





    }
}
