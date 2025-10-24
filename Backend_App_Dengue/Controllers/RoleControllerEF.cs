using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("Role")]
    [ApiController]
    public class RoleControllerEF : ControllerBase
    {
        private readonly IRepository<Role> _roleRepository;

        public RoleControllerEF(IRepository<Role> roleRepository)
        {
            _roleRepository = roleRepository;
        }

        /// <summary>
        /// Obtiene todos los roles
        /// </summary>
        [HttpGet]
        [Route("getRoles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _roleRepository.GetAllAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener roles", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un rol por ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(id);

                if (role == null)
                {
                    return NotFound(new { message = "Rol no encontrado" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el rol", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo rol
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] Role role)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(role.Name))
                {
                    return BadRequest(new { message = "El nombre del rol es requerido" });
                }

                var createdRole = await _roleRepository.AddAsync(role);
                return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el rol", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un rol existente
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role role)
        {
            try
            {
                var existingRole = await _roleRepository.GetByIdAsync(id);

                if (existingRole == null)
                {
                    return NotFound(new { message = "Rol no encontrado" });
                }

                existingRole.Name = role.Name;
                existingRole.IsActive = role.IsActive;

                await _roleRepository.UpdateAsync(existingRole);
                return Ok(new { message = "Rol actualizado con éxito", role = existingRole });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el rol", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un rol
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(id);

                if (role == null)
                {
                    return NotFound(new { message = "Rol no encontrado" });
                }

                await _roleRepository.DeleteAsync(role);
                return Ok(new { message = "Rol eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el rol", error = ex.Message });
            }
        }
    }
}
