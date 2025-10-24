using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    /// <summary>
    /// Controlador para gestión de usuarios del sistema
    /// </summary>
    [Route("User")]
    [ApiController]
    [Produces("application/json")]
    public class UserControllerEF : ControllerBase
    {
        private readonly IRepository<User> _userRepository;
        private readonly AppDbContext _context;

        public UserControllerEF(IRepository<User> userRepository, AppDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los usuarios con sus entidades relacionadas
        /// </summary>
        /// <returns>Lista de todos los usuarios con rol, ciudad, departamento, tipo de sangre y género</returns>
        /// <response code="200">Lista de usuarios retornada exitosamente</response>
        /// <response code="500">Error al obtener los usuarios</response>
        [HttpGet]
        [Route("getUsers")]
        [ProducesResponseType(typeof(List<UserResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.City).ThenInclude(c => c.Department)
                    .Include(u => u.BloodType)
                    .Include(u => u.Genre)
                    .ToListAsync();

                var userDtos = users.Select(u => u.ToResponseDto()).ToList();
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un usuario específico por ID con entidades relacionadas
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Datos completos del usuario incluyendo rol, ciudad, departamento, tipo de sangre y género</returns>
        /// <response code="200">Usuario encontrado y retornado exitosamente</response>
        /// <response code="400">El ID del usuario es requerido o inválido</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error al obtener el usuario</response>
        [HttpGet]
        [Route("getUser")]
        [ProducesResponseType(typeof(UserResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUser([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "El ID del usuario es requerido" });
            }

            try
            {
                if (!int.TryParse(id, out int userId))
                {
                    return BadRequest(new { message = "El ID del usuario debe ser un número válido" });
                }

                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.City).ThenInclude(c => c.Department)
                    .Include(u => u.BloodType)
                    .Include(u => u.Genre)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { message = "No se ha encontrado el usuario" });
                }

                var userDto = user.ToResponseDto();
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene usuarios sanos (sin casos activos de dengue)
        /// </summary>
        /// <returns>Lista de usuarios activos que no tienen casos de dengue activos</returns>
        /// <response code="200">Lista de usuarios sanos retornada exitosamente</response>
        /// <response code="500">Error al obtener los usuarios sanos</response>
        [HttpGet]
        [Route("getUserLive")]
        [ProducesResponseType(typeof(List<UserResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUserLive()
        {
            try
            {
                var healthyUsers = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.City).ThenInclude(c => c.Department)
                    .Include(u => u.BloodType)
                    .Include(u => u.Genre)
                    .Include(u => u.CasesAsPatient)
                    .Where(u => u.IsActive && !u.CasesAsPatient.Any(c => c.IsActive))
                    .ToListAsync();

                var userDtos = healthyUsers.Select(u => u.ToResponseDto()).ToList();
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios sanos", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-004: Actualiza el perfil propio del usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="dto">Datos a actualizar del perfil</param>
        /// <returns>Confirmación de actualización</returns>
        /// <response code="200">Perfil actualizado exitosamente</response>
        /// <response code="400">Los datos del usuario son requeridos</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="409">El correo ya se encuentra registrado</response>
        /// <response code="500">Error al actualizar el perfil</response>
        [HttpPut]
        [Route("updateProfile/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos del usuario son requeridos" });
            }

            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    user.Name = dto.Nombre;
                }

                if (!string.IsNullOrWhiteSpace(dto.Correo))
                {
                    // Check if email already exists for another user
                    var existingUser = await _userRepository.FirstOrDefaultAsync(u =>
                        u.Email == dto.Correo && u.Id != id
                    );

                    if (existingUser != null)
                    {
                        return Conflict(new { message = "El correo ya se encuentra registrado" });
                    }

                    user.Email = dto.Correo;
                }

                if (!string.IsNullOrWhiteSpace(dto.Direccion))
                {
                    user.Address = dto.Direccion;
                }

                if (dto.IdRol.HasValue)
                {
                    user.RoleId = dto.IdRol.Value;
                }

                if (dto.IdMunicipio.HasValue)
                {
                    user.CityId = dto.IdMunicipio;
                }

                if (dto.IdGenero.HasValue)
                {
                    user.GenreId = dto.IdGenero.Value;
                }

                await _userRepository.UpdateAsync(user);

                return Ok(new { message = "Perfil actualizado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el perfil", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-005: Actualiza un usuario (Solo administradores)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="dto">Datos a actualizar</param>
        /// <returns>Confirmación de actualización</returns>
        /// <response code="200">Usuario actualizado exitosamente</response>
        /// <response code="400">Los datos del usuario son requeridos</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="409">El correo ya se encuentra registrado</response>
        /// <response code="500">Error al actualizar el usuario</response>
        [HttpPut]
        [Route("updateUser/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            // Same logic as UpdateProfile - admin has same update capabilities
            return await UpdateProfile(id, dto);
        }

        /// <summary>
        /// HU-005: Elimina un usuario (Solo administradores) - Soft delete
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        /// <response code="200">Usuario eliminado exitosamente</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error al eliminar el usuario</response>
        /// <remarks>
        /// Realiza eliminación lógica marcando el usuario como inactivo (IsActive = false).
        /// No elimina físicamente el registro de la base de datos.
        /// </remarks>
        [HttpDelete]
        [Route("deleteUser/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Soft delete - mark as inactive
                user.IsActive = false;
                await _userRepository.UpdateAsync(user);

                return Ok(new { message = "Usuario eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-005: Busca usuarios con filtros opcionales
        /// </summary>
        /// <param name="filter">Texto para buscar en nombre o email</param>
        /// <param name="roleId">ID del rol para filtrar</param>
        /// <returns>Lista de usuarios que coinciden con los filtros</returns>
        /// <response code="200">Búsqueda completada exitosamente</response>
        /// <response code="500">Error al buscar usuarios</response>
        /// <remarks>
        /// Puede filtrar por nombre/email, por rol, o por ambos simultáneamente.
        /// Si no se proporcionan filtros, retorna todos los usuarios.
        /// </remarks>
        [HttpGet]
        [Route("searchUsers")]
        [ProducesResponseType(typeof(List<UserResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchUsers([FromQuery] string? filter, [FromQuery] int? roleId)
        {
            try
            {
                IQueryable<User> query = _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.City).ThenInclude(c => c.Department)
                    .Include(u => u.BloodType)
                    .Include(u => u.Genre);

                if (!string.IsNullOrWhiteSpace(filter) && roleId.HasValue)
                {
                    // Filter by name/email AND role
                    query = query.Where(u =>
                        u.RoleId == roleId.Value &&
                        (u.Name.Contains(filter) || u.Email.Contains(filter))
                    );
                }
                else if (!string.IsNullOrWhiteSpace(filter))
                {
                    // Filter by name/email only
                    query = query.Where(u =>
                        u.Name.Contains(filter) || u.Email.Contains(filter)
                    );
                }
                else if (roleId.HasValue)
                {
                    // Filter by role only
                    query = query.Where(u => u.RoleId == roleId.Value);
                }

                var users = await query.ToListAsync();
                var userDtos = users.Select(u => u.ToResponseDto()).ToList();

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar usuarios", error = ex.Message });
            }
        }
    }
}
