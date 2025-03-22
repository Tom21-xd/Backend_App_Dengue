using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        internal Connection cn = new Connection();

        [HttpGet]
        [Route("getRoles")]
        public async Task<IActionResult> getRoles()
        {
            DataTable roles = cn.ProcedimientosSelect(null, "ListarRoles", null);
            List<RoleModel> rolesList = roles.DataTableToList<RoleModel>();
            return Ok(rolesList);
        }
    }
}
