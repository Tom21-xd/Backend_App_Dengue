using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("Notification")]
    [ApiController]
    public class NotificationControllerEF : ControllerBase
    {
        private readonly IRepository<Notification> _notificationRepository;

        public NotificationControllerEF(IRepository<Notification> notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        /// <summary>
        /// Get all notifications
        /// </summary>
        [HttpGet]
        [Route("getNotifications")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var notifications = await _notificationRepository.GetAllAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las notificaciones", error = ex.Message });
            }
        }

        /// <summary>
        /// Get only unread notifications
        /// </summary>
        [HttpGet]
        [Route("getUnread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var unreadNotifications = await _notificationRepository.FindAsync(n => !n.IsRead && n.IsActive);
                return Ok(unreadNotifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener notificaciones no leídas", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPut]
        [Route("markAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);

                if (notification == null)
                {
                    return NotFound(new { message = "Notificación no encontrada" });
                }

                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);

                return Ok(new { message = "Notificación marcada como leída" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al marcar la notificación como leída", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPut]
        [Route("markAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                // Get all unread notifications
                var unreadNotifications = await _notificationRepository.FindAsync(n => !n.IsRead);

                // Mark each as read
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                // Update in batch would be better, but with current repository pattern we need to update individually
                foreach (var notification in unreadNotifications)
                {
                    await _notificationRepository.UpdateAsync(notification);
                }

                return Ok(new { message = "Todas las notificaciones han sido marcadas como leídas" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al marcar todas las notificaciones como leídas", error = ex.Message });
            }
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);

                if (notification == null)
                {
                    return NotFound(new { message = "Notificación no encontrada" });
                }

                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la notificación", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new notification
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] Notification notification)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(notification.Content))
                {
                    return BadRequest(new { message = "El contenido de la notificación es requerido" });
                }

                var createdNotification = await _notificationRepository.AddAsync(notification);
                return CreatedAtAction(nameof(GetNotificationById), new { id = createdNotification.Id }, createdNotification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear la notificación", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);

                if (notification == null)
                {
                    return NotFound(new { message = "Notificación no encontrada" });
                }

                await _notificationRepository.DeleteAsync(notification);
                return Ok(new { message = "Notificación eliminada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la notificación", error = ex.Message });
            }
        }
    }
}
