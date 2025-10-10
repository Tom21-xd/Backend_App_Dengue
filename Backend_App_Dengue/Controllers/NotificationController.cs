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
            try
            {
                DataTable usu = cn.ProcedimientosSelect(null, "ListarNotificaciones", null);
                List<NotificationModel> notificaciones = usu.DataTableToList<NotificationModel>();

                if (notificaciones == null)
                {
                    return Ok(new List<NotificationModel>());
                }

                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las notificaciones", error = ex.Message });
            }
        }

        // Obtener solo notificaciones no leídas
        [HttpGet]
        [Route("getUnread")]
        [ProducesResponseType(typeof(List<NotificationModel>), 200)]
        public IActionResult GetUnreadNotifications()
        {
            try
            {
                DataTable dt = cn.ProcedimientosSelect(null, "ObtenerNotificacionesNoLeidas", null);
                List<NotificationModel> notificaciones = dt.DataTableToList<NotificationModel>();

                if (notificaciones == null)
                {
                    return Ok(new List<NotificationModel>());
                }

                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener notificaciones no leídas", error = ex.Message });
            }
        }

        // Marcar una notificación como leída
        [HttpPut]
        [Route("markAsRead/{id}")]
        public IActionResult MarkAsRead(int id)
        {
            try
            {
                string[] parametros = { "idn" };
                string[] valores = { id.ToString() };

                cn.procedimientosInEd(parametros, "MarcarNotificacionLeida", valores);

                return Ok(new { message = "Notificación marcada como leída" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al marcar la notificación como leída", error = ex.Message });
            }
        }

        // Marcar todas las notificaciones como leídas
        [HttpPut]
        [Route("markAllAsRead")]
        public IActionResult MarkAllAsRead()
        {
            try
            {
                cn.procedimientosInEd(null, "MarcarTodasLeidas", null);

                return Ok(new { message = "Todas las notificaciones han sido marcadas como leídas" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al marcar todas las notificaciones como leídas", error = ex.Message });
            }
        }
    }
}
