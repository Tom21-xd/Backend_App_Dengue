using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("User")]
    [ApiController]
    public class UserControllerEF : ControllerBase
    {
        private readonly IRepository<User> _userRepository;

        public UserControllerEF(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        [Route("getUsers")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet]
        [Route("getUser")]
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

                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "No se ha encontrado el usuario" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Get healthy users (users without active dengue cases)
        /// </summary>
        [HttpGet]
        [Route("getUserLive")]
        public async Task<IActionResult> GetUserLive()
        {
            try
            {
                // Get all users who don't have any active cases as patients
                var healthyUsers = await _userRepository.FindAsync(u =>
                    u.IsActive &&
                    !u.CasesAsPatient.Any(c => c.IsActive)
                );

                return Ok(healthyUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios sanos", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-004: Update own profile
        /// </summary>
        [HttpPut]
        [Route("updateProfile/{id}")]
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
        /// HU-005: Update user (Admin)
        /// </summary>
        [HttpPut]
        [Route("updateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            // Same logic as UpdateProfile - admin has same update capabilities
            return await UpdateProfile(id, dto);
        }

        /// <summary>
        /// HU-005: Delete user (Admin) - Soft delete
        /// </summary>
        [HttpDelete]
        [Route("deleteUser/{id}")]
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
        /// HU-005: Search users with filters
        /// </summary>
        [HttpGet]
        [Route("searchUsers")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? filter, [FromQuery] int? roleId)
        {
            try
            {
                IEnumerable<User> users;

                if (!string.IsNullOrWhiteSpace(filter) && roleId.HasValue)
                {
                    // Filter by name/email AND role
                    users = await _userRepository.FindAsync(u =>
                        u.RoleId == roleId.Value &&
                        (u.Name.Contains(filter) || u.Email.Contains(filter))
                    );
                }
                else if (!string.IsNullOrWhiteSpace(filter))
                {
                    // Filter by name/email only
                    users = await _userRepository.FindAsync(u =>
                        u.Name.Contains(filter) || u.Email.Contains(filter)
                    );
                }
                else if (roleId.HasValue)
                {
                    // Filter by role only
                    users = await _userRepository.FindAsync(u => u.RoleId == roleId.Value);
                }
                else
                {
                    // No filter - get all
                    users = await _userRepository.GetAllAsync();
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar usuarios", error = ex.Message });
            }
        }
    }
}
