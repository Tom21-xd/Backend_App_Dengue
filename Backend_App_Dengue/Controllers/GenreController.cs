using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        internal Connection cn = new Connection();

        [HttpGet]
        [Route("getGenres")]
        public async Task<IActionResult> getGenres()
        {
            DataTable gen = cn.ProcedimientosSelect(null, "ListarGenero", null);
            List<GenreModel> generos = gen.DataTableToList<GenreModel>();
            return Ok(generos);
        }
    }
}
