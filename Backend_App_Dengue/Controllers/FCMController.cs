using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FCMController : ControllerBase
    {
        internal Connection cn = new Connection();

        // POST: FCM/saveToken
        // Guardar o actualizar el token FCM de un usuario
        [HttpPost]
        [Route("saveToken")]
        public IActionResult SaveFCMToken([FromBody] FCMTokenModel request)
        {
            try
            {
                if (request == null || request.IdUsuario <= 0 || string.IsNullOrEmpty(request.FcmToken))
                {
                    return BadRequest(new { message = "Datos inválidos" });
                }

                string[] parametros = { "idUsuario", "fcmToken" };
                string[] valores = { request.IdUsuario.ToString(), request.FcmToken };

                cn.procedimientosInEd(parametros, "GuardarFCMToken", valores);

                return Ok(new { message = "Token FCM guardado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al guardar el token FCM", error = ex.Message });
            }
        }

        // DELETE: FCM/deleteToken/{userId}
        // Eliminar el token FCM de un usuario (cuando cierra sesión)
        [HttpDelete]
        [Route("deleteToken/{userId}")]
        public IActionResult DeleteFCMToken(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "ID de usuario inválido" });
                }

                string[] parametros = { "idUsuario" };
                string[] valores = { userId.ToString() };

                cn.procedimientosInEd(parametros, "EliminarFCMToken", valores);

                return Ok(new { message = "Token FCM eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el token FCM", error = ex.Message });
            }
        }

        // GET: FCM/getToken/{userId}
        // Obtener el token FCM de un usuario
        [HttpGet]
        [Route("getToken/{userId}")]
        public IActionResult GetFCMToken(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "ID de usuario inválido" });
                }

                string[] parametros = { "idUsuario" };
                string[] valores = { userId.ToString() };

                DataTable dt = cn.ProcedimientosSelect(parametros, "ObtenerFCMToken", valores);

                if (dt.Rows.Count == 0)
                {
                    return NotFound(new { message = "Token FCM no encontrado para este usuario" });
                }

                string token = dt.Rows[0]["FCM_TOKEN"].ToString();
                return Ok(new { fcm_token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el token FCM", error = ex.Message });
            }
        }
    }
}
