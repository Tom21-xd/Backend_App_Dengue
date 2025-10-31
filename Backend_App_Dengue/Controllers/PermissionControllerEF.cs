using Backend_App_Dengue.Attributes;
using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    /// <summary>
    /// Controlador para gestión de permisos del sistema
    /// Solo accesible por administradores
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionControllerEF : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PermissionControllerEF> _logger;

        public PermissionControllerEF(AppDbContext context, ILogger<PermissionControllerEF> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los permisos del sistema agrupados por categoría
        /// Solo requiere autenticación, sin permiso específico necesario
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                var permissions = await _context.Permissions
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Name)
                    .ToListAsync();

                var grouped = permissions
                    .GroupBy(p => p.Category ?? "Sin categoría")
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalPermissions = g.Count(),
                        Permissions = g.Select(p => new
                        {
                            id = p.Id,
                            name = p.Name,
                            description = p.Description,
                            code = p.Code,
                            category = p.Category,
                            isActive = p.IsActive
                        })
                    });

                return Ok(new { data = grouped });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos");
                return StatusCode(500, new { message = "Error al obtener los permisos" });
            }
        }

        /// <summary>
        /// Obtiene los permisos asignados a un rol específico
        /// Solo requiere autenticación
        /// </summary>
        [HttpGet("role/{roleId}")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == roleId && r.IsActive);

                if (role == null)
                {
                    return NotFound(new { message = "Rol no encontrado" });
                }

                var permissions = role.RolePermissions
                    .Where(rp => rp.IsActive && rp.Permission.IsActive)
                    .Select(rp => new
                    {
                        id = rp.Permission.Id,
                        name = rp.Permission.Name,
                        description = rp.Permission.Description,
                        code = rp.Permission.Code,
                        category = rp.Permission.Category,
                        isActive = rp.Permission.IsActive
                    })
                    .OrderBy(p => p.category)
                    .ThenBy(p => p.name)
                    .ToList();

                return Ok(new
                {
                    roleId = role.Id,
                    roleName = role.Name,
                    totalPermissions = permissions.Count,
                    permissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del rol {RoleId}", roleId);
                return StatusCode(500, new { message = "Error al obtener los permisos del rol" });
            }
        }

        /// <summary>
        /// Actualiza los permisos de un rol (asignar/revocar)
        /// </summary>
        [HttpPut("role/{roleId}")]
        [RequirePermission(PermissionCode.PERMISSION_MANAGE)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] UpdateRolePermissionsRequest request)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == roleId && r.IsActive);

                if (role == null)
                {
                    return NotFound(new { message = "Rol no encontrado" });
                }

                // Desactivar todos los permisos actuales del rol
                foreach (var rp in role.RolePermissions)
                {
                    rp.IsActive = false;
                }

                // Asignar nuevos permisos
                foreach (var permissionId in request.PermissionIds)
                {
                    var existingRolePermission = role.RolePermissions
                        .FirstOrDefault(rp => rp.PermissionId == permissionId);

                    if (existingRolePermission != null)
                    {
                        // Reactivar si ya existe
                        existingRolePermission.IsActive = true;
                        existingRolePermission.AssignedAt = DateTime.Now;
                    }
                    else
                    {
                        // Crear nuevo
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = roleId,
                            PermissionId = permissionId,
                            AssignedAt = DateTime.Now,
                            IsActive = true
                        });
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Permisos del rol {RoleId} actualizados correctamente", roleId);

                return Ok(new
                {
                    message = "Permisos actualizados correctamente",
                    roleId,
                    totalPermissions = request.PermissionIds.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permisos del rol {RoleId}", roleId);
                return StatusCode(500, new { message = "Error al actualizar los permisos" });
            }
        }

        /// <summary>
        /// Obtiene la matriz de permisos (todos los roles vs todos los permisos)
        /// </summary>
        [HttpGet("matrix")]
        [RequirePermission(PermissionCode.PERMISSION_MANAGE)]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetPermissionsMatrix()
        {
            try
            {
                var roles = await _context.Roles
                    .Where(r => r.IsActive)
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .ToListAsync();

                var allPermissions = await _context.Permissions
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Name)
                    .ToListAsync();

                var matrix = allPermissions.Select(permission => new
                {
                    permissionId = permission.Id,
                    permissionName = permission.Name,
                    category = permission.Category,
                    roles = roles.Select(role => new
                    {
                        roleId = role.Id,
                        roleName = role.Name,
                        hasPermission = role.RolePermissions.Any(rp =>
                            rp.PermissionId == permission.Id &&
                            rp.IsActive &&
                            rp.Permission.IsActive)
                    })
                });

                return Ok(new { data = matrix });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener matriz de permisos");
                return StatusCode(500, new { message = "Error al obtener la matriz de permisos" });
            }
        }

        /// <summary>
        /// Obtiene todos los permisos del usuario autenticado
        /// </summary>
        [HttpGet("current-user")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetCurrentUserPermissions()
        {
            try
            {
                _logger.LogInformation("=== GET CURRENT USER PERMISSIONS ===");
                _logger.LogInformation($"Claims en el request: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub")
                               ?? User.FindFirst("userId")
                               ?? User.FindFirst("UserId");

                if (userIdClaim == null)
                {
                    _logger.LogWarning("No se encontró claim de userId en el token");
                    return Unauthorized(new { message = "Usuario no autenticado - No se encontró userId en el token" });
                }

                _logger.LogInformation($"userId claim encontrado: {userIdClaim.Type} = {userIdClaim.Value}");

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning($"No se pudo parsear userId: {userIdClaim.Value}");
                    return Unauthorized(new { message = "Usuario no autenticado - userId inválido" });
                }

                _logger.LogInformation($"Buscando usuario con ID: {userId}");

                var user = await _context.Users
                    .Include(u => u.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning($"Usuario no encontrado o inactivo - ID: {userId}");
                    return NotFound(new { message = $"Usuario no encontrado - ID: {userId}" });
                }

                _logger.LogInformation($"Usuario encontrado: {user.Name}, Rol: {user.Role.Name} (ID: {user.RoleId})");
                _logger.LogInformation($"RolePermissions count: {user.Role.RolePermissions.Count}");

                var permissions = user.Role.RolePermissions
                    .Where(rp => rp.IsActive && rp.Permission.IsActive)
                    .Select(rp => rp.Permission.Code)
                    .Distinct()
                    .OrderBy(code => code)
                    .ToList();

                _logger.LogInformation($"Permisos activos: {permissions.Count}");
                _logger.LogInformation($"Permisos: {string.Join(", ", permissions)}");

                return Ok(new
                {
                    userId = user.Id,
                    roleId = user.RoleId,
                    roleName = user.Role.Name,
                    permissions = permissions,
                    totalPermissions = permissions.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del usuario actual");
                return StatusCode(500, new { message = "Error al obtener los permisos del usuario" });
            }
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico
        /// </summary>
        [HttpGet("check/{permissionCode}")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> CheckUserPermission(string permissionCode)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub")
                               ?? User.FindFirst("userId");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var hasPermission = await _context.Users
                    .Where(u => u.Id == userId && u.IsActive)
                    .SelectMany(u => u.Role.RolePermissions)
                    .AnyAsync(rp => rp.Permission.Code == permissionCode
                                 && rp.IsActive
                                 && rp.Permission.IsActive);

                return Ok(new
                {
                    userId,
                    permissionCode,
                    hasPermission
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar permiso {PermissionCode}", permissionCode);
                return StatusCode(500, new { message = "Error al verificar el permiso" });
            }
        }
    }

    /// <summary>
    /// DTO para actualizar permisos de un rol
    /// </summary>
    public class UpdateRolePermissionsRequest
    {
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}
