using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model.Dto;
using Backend_App_Dengue.Services;
using Backend_App_Dengue.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    /// <summary>
    /// Controlador para gestión de solicitudes de aprobación de usuarios
    /// Requiere autenticación y permisos específicos
    /// </summary>
    [Route("UserApproval")]
    [ApiController]
    [Produces("application/json")]
    [Authorize] // Requiere estar autenticado
    public class UserApprovalController : ControllerBase
    {
        private readonly IRepository<UserApprovalRequest> _approvalRequestRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly AppDbContext _context;

        public UserApprovalController(
            IRepository<UserApprovalRequest> approvalRequestRepository,
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            AppDbContext context)
        {
            _approvalRequestRepository = approvalRequestRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _context = context;
        }

        /// <summary>
        /// Obtiene todas las solicitudes de aprobación pendientes
        /// </summary>
        /// <returns>Lista de solicitudes pendientes</returns>
        /// <response code="200">Lista de solicitudes obtenida exitosamente</response>
        /// <response code="403">No tiene permiso para ver solicitudes</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [Route("pending")]
        [RequirePermission("USER_APPROVAL_VIEW")]
        [ProducesResponseType(typeof(List<ApprovalRequestDto>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                var pendingRequests = await _context.UserApprovalRequests
                    .Include(ar => ar.User)
                    .Include(ar => ar.RequestedRole)
                    .Where(ar => ar.Status == "PENDIENTE")
                    .OrderBy(ar => ar.RequestDate)
                    .ToListAsync();

                var result = pendingRequests.Select(ar => new ApprovalRequestDto
                {
                    Id = ar.Id,
                    UserId = ar.UserId,
                    UserName = ar.User.Name,
                    UserEmail = ar.User.Email,
                    Status = ar.Status,
                    RequestedRoleId = ar.RequestedRoleId,
                    RequestedRoleName = ar.RequestedRole.Name,
                    RejectionReason = ar.RejectionReason,
                    ApprovedByAdminId = ar.ApprovedByAdminId,
                    RequestDate = ar.RequestDate,
                    ResolutionDate = ar.ResolutionDate,
                    RethusData = ar.RethusData,
                    RethusError = ar.RethusError
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener solicitudes pendientes", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las solicitudes de aprobación (historial completo)
        /// </summary>
        /// <returns>Lista de todas las solicitudes</returns>
        /// <response code="200">Lista de solicitudes obtenida exitosamente</response>
        /// <response code="403">No tiene permiso para ver historial</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [Route("all")]
        [RequirePermission("USER_APPROVAL_HISTORY")]
        [ProducesResponseType(typeof(List<ApprovalRequestDto>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllApprovals()
        {
            try
            {
                var allRequests = await _context.UserApprovalRequests
                    .Include(ar => ar.User)
                    .Include(ar => ar.RequestedRole)
                    .Include(ar => ar.ApprovedByAdmin)
                    .OrderByDescending(ar => ar.RequestDate)
                    .ToListAsync();

                var result = allRequests.Select(ar => new ApprovalRequestDto
                {
                    Id = ar.Id,
                    UserId = ar.UserId,
                    UserName = ar.User.Name,
                    UserEmail = ar.User.Email,
                    Status = ar.Status,
                    RequestedRoleId = ar.RequestedRoleId,
                    RequestedRoleName = ar.RequestedRole.Name,
                    RejectionReason = ar.RejectionReason,
                    ApprovedByAdminId = ar.ApprovedByAdminId,
                    ApprovedByAdminName = ar.ApprovedByAdmin?.Name,
                    RequestDate = ar.RequestDate,
                    ResolutionDate = ar.ResolutionDate,
                    RethusData = ar.RethusData,
                    RethusError = ar.RethusError
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener solicitudes", error = ex.Message });
            }
        }

        /// <summary>
        /// Aprueba una solicitud de usuario y le cambia el rol
        /// </summary>
        /// <param name="adminId">ID del administrador que aprueba</param>
        /// <param name="dto">Datos de aprobación</param>
        /// <returns>Confirmación de aprobación</returns>
        /// <response code="200">Usuario aprobado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="403">No tiene permiso para aprobar usuarios</response>
        /// <response code="404">Usuario o solicitud no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [Route("approve/{adminId}")]
        [RequirePermission("USER_APPROVAL_APPROVE")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        public async Task<IActionResult> ApproveUser(int adminId, [FromBody] ApproveUserDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos de aprobación son requeridos" });
            }

            try
            {
                // Buscar usuario
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Buscar solicitud pendiente del usuario
                var approvalRequest = await _context.UserApprovalRequests
                    .Include(ar => ar.RequestedRole)
                    .FirstOrDefaultAsync(ar => ar.UserId == dto.UserId && ar.Status == "PENDIENTE");

                if (approvalRequest == null)
                {
                    return NotFound(new { message = "No hay solicitud pendiente para este usuario" });
                }

                // Verificar que el nuevo rol existe
                var newRole = await _roleRepository.GetByIdAsync(dto.NewRoleId);
                if (newRole == null)
                {
                    return BadRequest(new { message = "El rol especificado no existe" });
                }

                // Actualizar rol del usuario
                user.RoleId = dto.NewRoleId;
                await _userRepository.UpdateAsync(user);

                // Actualizar solicitud de aprobación
                approvalRequest.Status = "APROBADO";
                approvalRequest.ApprovedByAdminId = adminId;
                approvalRequest.ResolutionDate = DateTime.Now;
                await _approvalRequestRepository.UpdateAsync(approvalRequest);

                // Enviar email de notificación
                try
                {
                    ServiceGmail emailService = new ServiceGmail();
                    string htmlBody = EmailTemplates.ApprovalTemplate(user.Name, user.Email, newRole.Name);
                    await Task.Run(() => emailService.SendEmailGmail(
                        user.Email,
                        "¡Tu cuenta ha sido aprobada! - Dengue Track",
                        htmlBody
                    ));
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Error al enviar email de aprobación: {emailEx.Message}");
                }

                return Ok(new
                {
                    message = "Usuario aprobado exitosamente",
                    usuario = new
                    {
                        id = user.Id,
                        nombre = user.Name,
                        email = user.Email,
                        nuevoRol = newRole.Name
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al aprobar usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Rechaza una solicitud de usuario
        /// </summary>
        /// <param name="adminId">ID del administrador que rechaza</param>
        /// <param name="dto">Datos de rechazo</param>
        /// <returns>Confirmación de rechazo</returns>
        /// <response code="200">Solicitud rechazada exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="403">No tiene permiso para rechazar usuarios</response>
        /// <response code="404">Usuario o solicitud no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [Route("reject/{adminId}")]
        [RequirePermission("USER_APPROVAL_REJECT")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        public async Task<IActionResult> RejectUser(int adminId, [FromBody] RejectUserDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.RejectionReason))
            {
                return BadRequest(new { message = "El motivo de rechazo es requerido" });
            }

            try
            {
                // Buscar usuario
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Buscar solicitud pendiente del usuario
                var approvalRequest = await _context.UserApprovalRequests
                    .FirstOrDefaultAsync(ar => ar.UserId == dto.UserId && ar.Status == "PENDIENTE");

                if (approvalRequest == null)
                {
                    return NotFound(new { message = "No hay solicitud pendiente para este usuario" });
                }

                // Actualizar solicitud de aprobación
                approvalRequest.Status = "RECHAZADO";
                approvalRequest.ApprovedByAdminId = adminId;
                approvalRequest.RejectionReason = dto.RejectionReason;
                approvalRequest.ResolutionDate = DateTime.Now;
                await _approvalRequestRepository.UpdateAsync(approvalRequest);

                // Enviar email de notificación
                try
                {
                    ServiceGmail emailService = new ServiceGmail();
                    string htmlBody = EmailTemplates.RejectionTemplate(user.Name, user.Email, dto.RejectionReason);
                    await Task.Run(() => emailService.SendEmailGmail(
                        user.Email,
                        "Actualización sobre tu solicitud - Dengue Track",
                        htmlBody
                    ));
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Error al enviar email de rechazo: {emailEx.Message}");
                }

                return Ok(new
                {
                    message = "Solicitud rechazada exitosamente",
                    usuario = new
                    {
                        id = user.Id,
                        nombre = user.Name,
                        email = user.Email
                    },
                    motivo = dto.RejectionReason
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al rechazar solicitud", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el estado de aprobación de un usuario específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Estado de aprobación del usuario</returns>
        /// <response code="200">Estado obtenido exitosamente</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [Route("status/{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUserApprovalStatus(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                var latestRequest = await _context.UserApprovalRequests
                    .Include(ar => ar.RequestedRole)
                    .Where(ar => ar.UserId == userId)
                    .OrderByDescending(ar => ar.RequestDate)
                    .FirstOrDefaultAsync();

                if (latestRequest == null)
                {
                    return Ok(new
                    {
                        userId = userId,
                        tieneSolicitud = false,
                        mensaje = "El usuario no tiene solicitudes de aprobación"
                    });
                }

                return Ok(new
                {
                    userId = userId,
                    tieneSolicitud = true,
                    solicitud = new ApprovalRequestDto
                    {
                        Id = latestRequest.Id,
                        UserId = latestRequest.UserId,
                        UserName = user.Name,
                        UserEmail = user.Email,
                        Status = latestRequest.Status,
                        RequestedRoleId = latestRequest.RequestedRoleId,
                        RequestedRoleName = latestRequest.RequestedRole.Name,
                        RejectionReason = latestRequest.RejectionReason,
                        RequestDate = latestRequest.RequestDate,
                        ResolutionDate = latestRequest.ResolutionDate,
                        RethusData = latestRequest.RethusData,
                        RethusError = latestRequest.RethusError
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estado de aprobación", error = ex.Message });
            }
        }
    }
}
