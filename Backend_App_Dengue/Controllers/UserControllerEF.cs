using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    [Route("User")]
    [ApiController]
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
        /// Get all users with related entities
        /// </summary>
        [HttpGet]
        [Route("getUsers")]
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
        /// Get user by ID with related entities
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
        /// Get healthy users (users without active dengue cases)
        /// </summary>
        [HttpGet]
        [Route("getUserLive")]
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
