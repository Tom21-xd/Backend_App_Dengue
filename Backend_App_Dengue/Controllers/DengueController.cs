using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DengueController : ControllerBase
    {
        internal Connection cn = new Connection();

        [HttpGet]
        [Route("getSymptoms")]
        public async Task<IActionResult> getSymptoms()
        {
            DataTable tb = cn.ProcedimientosSelect(null, "ListarSintomas", null);
            List<SymptomModel> sintomas = tb.DataTableToList<SymptomModel>();
            return Ok(sintomas);
        }

        [HttpGet]
        [Route("getTypesOfDengue")]
        public async Task<IActionResult> getTypesOfDengue()
        {
            DataTable tb = cn.ProcedimientosSelect(null, "ListarTIpoDengue", null);
            List<TypeOfDengueModel> tiposDengue = tb.DataTableToList<TypeOfDengueModel>();
            return Ok(tiposDengue);
        }

        [HttpGet]
        [Route("getTypesOfBlood")]
        public async Task<IActionResult> getTypesOfBlood()
        {
            DataTable tb = cn.ProcedimientosSelect(null, "ListarTipoSangre", null);
            List<TypeOfBloodModel> tiposSangre = tb.DataTableToList<TypeOfBloodModel>();
            return Ok(tiposSangre);
        }


    }
}
