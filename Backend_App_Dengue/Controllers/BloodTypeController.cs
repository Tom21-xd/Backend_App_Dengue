using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BloodTypeController : Controller
    {
        internal Connection cn = new Connection();

        [HttpGet]
        [Route("getBloodType")]
        public async Task<IActionResult> getBloodType()
        {
            DataTable usu = cn.ProcedimientosSelect(null, "ListarTipoSangre", null);
            List<TypeOfBloodModel> usuarios = usu.DataTableToList<TypeOfBloodModel>();

            return Ok(usuarios);

        }
    }
}