using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        Connection cn = new Connection();

        [HttpGet]
        [Route("getPublications")]
        public async Task<IActionResult> getPublications()
        {
            DataTable dt = cn.ProcedimientosSelect(null, "ListarPublicaciones", null);
            List<PublicationModel> lista = dt.DataTableToList<PublicationModel>();
            return Ok(lista);
        }

    }
}
