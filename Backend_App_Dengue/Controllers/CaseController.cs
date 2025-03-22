using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
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
        public async Task<IActionResult> getCases()
        {
            DataTable tb = cn.ProcedimientosSelect(null, "ListarCasos", null);
            List<CaseModel> casos = tb.DataTableToList<CaseModel>();
            return Ok(casos);
        }

        [HttpPost]
        [Route("creatreCase")]
        public async Task<IActionResult> creatreCase(CreateCaseModel caso)
        {
            string[] parametros = { "descri", "ihospital", "tdengue", "paciente", "personalmedico", "direccion" };
            string[] aux = { caso.descripcion, caso.id_hospital + "", caso.id_tipoDengue + "", caso.id_paciente + "", caso.id_personalMedico + "", caso.direccion.ToString() };
            cn.procedimientosInEd(parametros, "CrearCaso", aux);
            return Ok(new { message = "Se ha creado el caso con exito" });
        }
    }
}
