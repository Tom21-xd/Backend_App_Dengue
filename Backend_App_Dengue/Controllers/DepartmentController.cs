using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        internal Connection cn = new Connection();

        [HttpGet]
        [Route("getDepartments")]
        public async Task<IActionResult> getDepartments()
        {
            DataTable dt = cn.ProcedimientosSelect(null, "ListarDepartamento", null);
            List<DepartmentModel> lista = dt.DataTableToList<DepartmentModel>();
            return Ok(lista);
        }

        [HttpPost]
        [Route("getCities")]
        public async Task<IActionResult> getCities(string filter)
        {
            string[] aux = { filter };
            string[] parametros = { "nombre" };
            DataTable dt = cn.ProcedimientosSelect(parametros, "ListarMunicipio", aux);
            List<CityModel> lista = dt.DataTableToList<CityModel>();
            return Ok(lista);
        }


    }
}
