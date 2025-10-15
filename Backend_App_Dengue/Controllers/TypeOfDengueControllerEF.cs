using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("Dengue")]
    [ApiController]
    public class TypeOfDengueControllerEF : ControllerBase
    {
        private readonly IRepository<TypeOfDengue> _dengueTypeRepository;

        public TypeOfDengueControllerEF(IRepository<TypeOfDengue> dengueTypeRepository)
        {
            _dengueTypeRepository = dengueTypeRepository;
        }

        /// <summary>
        /// Get all dengue types
        /// </summary>
        [HttpGet]
        [Route("getTypesOfDengue")]
        public async Task<IActionResult> GetTypesOfDengue()
        {
            try
            {
                var dengueTypes = await _dengueTypeRepository.GetAllAsync();
                return Ok(dengueTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener tipos de dengue", error = ex.Message });
            }
        }

        /// <summary>
        /// Get dengue type by ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetDengueTypeById(int id)
        {
            try
            {
                var dengueType = await _dengueTypeRepository.GetByIdAsync(id);

                if (dengueType == null)
                {
                    return NotFound(new { message = "Tipo de dengue no encontrado" });
                }

                return Ok(dengueType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el tipo de dengue", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new dengue type
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDengueType([FromBody] TypeOfDengue dengueType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dengueType.Name))
                {
                    return BadRequest(new { message = "El nombre del tipo de dengue es requerido" });
                }

                var createdDengueType = await _dengueTypeRepository.AddAsync(dengueType);
                return CreatedAtAction(nameof(GetDengueTypeById), new { id = createdDengueType.Id }, createdDengueType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el tipo de dengue", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing dengue type
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateDengueType(int id, [FromBody] TypeOfDengue dengueType)
        {
            try
            {
                var existingDengueType = await _dengueTypeRepository.GetByIdAsync(id);

                if (existingDengueType == null)
                {
                    return NotFound(new { message = "Tipo de dengue no encontrado" });
                }

                existingDengueType.Name = dengueType.Name;
                existingDengueType.IsActive = dengueType.IsActive;

                await _dengueTypeRepository.UpdateAsync(existingDengueType);
                return Ok(new { message = "Tipo de dengue actualizado con éxito", dengueType = existingDengueType });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el tipo de dengue", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a dengue type
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteDengueType(int id)
        {
            try
            {
                var dengueType = await _dengueTypeRepository.GetByIdAsync(id);

                if (dengueType == null)
                {
                    return NotFound(new { message = "Tipo de dengue no encontrado" });
                }

                await _dengueTypeRepository.DeleteAsync(dengueType);
                return Ok(new { message = "Tipo de dengue eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el tipo de dengue", error = ex.Message });
            }
        }
    }
}
