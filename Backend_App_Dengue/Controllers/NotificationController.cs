using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NotificationController : Controller
    {
        internal Connection cn = new Connection();

        [HttpGet]
        [Route("getNotifications")]
        public async Task<IActionResult> getNotifications()
        {
            DataTable usu = cn.ProcedimientosSelect(null, "ListarNotificaciones", null);
            List<NotificationModel> notificaciones = usu.DataTableToList<NotificationModel>();

            return Ok(notificaciones);
        }
    }
}
