using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend_App_Dengue.Attributes
{
    /// <summary>
    /// Atributo de autorización basado en permisos
    /// Valida que el usuario tenga el permiso especificado a través de su rol
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _permissionCode;

        public RequirePermissionAttribute(string permissionCode)
        {
            _permissionCode = permissionCode;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Obtener el ID del usuario del token JWT
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? context.HttpContext.User.FindFirst("sub")
                           ?? context.HttpContext.User.FindFirst("userId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Usuario no autenticado"
                });
                return;
            }

            // Obtener AppDbContext del DI
            var dbContext = context.HttpContext.RequestServices.GetService<AppDbContext>();
            if (dbContext == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            // Verificar que el usuario existe y tiene permisos
            var hasPermission = await dbContext.Users
                .Where(u => u.Id == userId && u.IsActive)
                .SelectMany(u => u.Role.RolePermissions)
                .AnyAsync(rp => rp.Permission.Code == _permissionCode
                             && rp.IsActive
                             && rp.Permission.IsActive);

            if (!hasPermission)
            {
                context.Result = new ObjectResult(new
                {
                    message = "No tienes permisos para realizar esta acción",
                    requiredPermission = _permissionCode
                })
                {
                    StatusCode = 403
                };
                return;
            }

            await next();
        }
    }
}
