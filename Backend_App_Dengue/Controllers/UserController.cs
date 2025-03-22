using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        internal Connection cn = new Connection();

        [HttpGet]
        [Route("getUsers")]
        public async Task<IActionResult> getUsers()
        {
            DataTable usu = cn.ProcedimientosSelect(null, "ListarUsuarios", null);
            List<UserModel> usuarios = usu.DataTableToList<UserModel>();

            return Ok(usuarios);
        }


    }
}
