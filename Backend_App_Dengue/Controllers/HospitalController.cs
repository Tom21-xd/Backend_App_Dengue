using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Runtime.Intrinsics.X86;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HospitalController : ControllerBase
    {
        internal Connection cn = new Connection();

        [HttpPost]
        [Route("filterHospitals")]
        public async Task<IActionResult> filterHospitals(string name)
        {
            string[] aux = { name };
            string[] parametros = { "nombre" };
            DataTable h = cn.ProcedimientosSelect(parametros, "FiltrarHospital", aux);
            List<HospitalModel> hospitals = h.DataTableToList<HospitalModel>();
            return Ok(hospitals);
        }

        [HttpGet]
        [Route("getHospitals")]
        public async Task<IActionResult> getHospitals()
        {
            DataTable h = cn.ProcedimientosSelect(null, "ListarHospi", null);
            List<HospitalModel> hospitals = h.DataTableToList<HospitalModel>();
            return Ok(hospitals);
        }

    }
}
