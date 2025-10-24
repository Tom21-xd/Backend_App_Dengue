using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("FCM")]
    [ApiController]
    public class FCMControllerEF : ControllerBase
    {
        private readonly IRepository<FCMToken> _fcmTokenRepository;

        public FCMControllerEF(IRepository<FCMToken> fcmTokenRepository)
        {
            _fcmTokenRepository = fcmTokenRepository;
        }

        /// <summary>
        /// Guarda o actualiza el token FCM de un usuario
        /// </summary>
        [HttpPost]
        [Route("saveToken")]
        public async Task<IActionResult> SaveFCMToken([FromBody] FCMTokenModel request)
        {
            try
            {
                if (request == null || request.IdUsuario <= 0 || string.IsNullOrEmpty(request.FcmToken))
                {
                    return BadRequest(new { message = "Datos inválidos" });
                }

                // Verificar si el token ya existe para este usuario
                var existingToken = await _fcmTokenRepository.FirstOrDefaultAsync(t => t.UserId == request.IdUsuario);

                if (existingToken != null)
                {
                    // Actualizar token existente
                    existingToken.Token = request.FcmToken;
                    existingToken.UpdatedAt = DateTime.Now;
                    await _fcmTokenRepository.UpdateAsync(existingToken);
                }
                else
                {
                    // Crear nuevo token
                    var newToken = new FCMToken
                    {
                        UserId = request.IdUsuario,
                        Token = request.FcmToken,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await _fcmTokenRepository.AddAsync(newToken);
                }

                return Ok(new { message = "Token FCM guardado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al guardar el token FCM", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina el token FCM de un usuario (al cerrar sesión)
        /// </summary>
        [HttpDelete]
        [Route("deleteToken/{userId}")]
        public async Task<IActionResult> DeleteFCMToken(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "ID de usuario inválido" });
                }

                var token = await _fcmTokenRepository.FirstOrDefaultAsync(t => t.UserId == userId);

                if (token == null)
                {
                    return NotFound(new { message = "Token FCM no encontrado para este usuario" });
                }

                await _fcmTokenRepository.DeleteAsync(token);

                return Ok(new { message = "Token FCM eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el token FCM", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el token FCM de un usuario
        /// </summary>
        [HttpGet]
        [Route("getToken/{userId}")]
        public async Task<IActionResult> GetFCMToken(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "ID de usuario inválido" });
                }

                var token = await _fcmTokenRepository.FirstOrDefaultAsync(t => t.UserId == userId);

                if (token == null)
                {
                    return NotFound(new { message = "Token FCM no encontrado para este usuario" });
                }

                return Ok(new { fcm_token = token.Token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el token FCM", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all FCM tokens
        /// </summary>
        [HttpGet]
        [Route("getAllTokens")]
        public async Task<IActionResult> GetAllTokens()
        {
            try
            {
                var tokens = await _fcmTokenRepository.GetAllAsync();
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener todos los tokens FCM", error = ex.Message });
            }
        }
    }
}
