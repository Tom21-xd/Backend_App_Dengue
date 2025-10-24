using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("BloodType")]
    [ApiController]
    public class TypeOfBloodControllerEF : ControllerBase
    {
        private readonly IRepository<TypeOfBlood> _bloodTypeRepository;

        public TypeOfBloodControllerEF(IRepository<TypeOfBlood> bloodTypeRepository)
        {
            _bloodTypeRepository = bloodTypeRepository;
        }

        /// <summary>
        /// Obtiene todos los tipos de sangre
        /// </summary>
        [HttpGet]
        [Route("getBloodType")]
        public async Task<IActionResult> GetBloodTypes()
        {
            try
            {
                var bloodTypes = await _bloodTypeRepository.GetAllAsync();
                return Ok(bloodTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener tipos de sangre", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un tipo de sangre por ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetBloodTypeById(int id)
        {
            try
            {
                var bloodType = await _bloodTypeRepository.GetByIdAsync(id);

                if (bloodType == null)
                {
                    return NotFound(new { message = "Tipo de sangre no encontrado" });
                }

                return Ok(bloodType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el tipo de sangre", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo tipo de sangre
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateBloodType([FromBody] TypeOfBlood bloodType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bloodType.Name))
                {
                    return BadRequest(new { message = "El nombre del tipo de sangre es requerido" });
                }

                var createdBloodType = await _bloodTypeRepository.AddAsync(bloodType);
                return CreatedAtAction(nameof(GetBloodTypeById), new { id = createdBloodType.Id }, createdBloodType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el tipo de sangre", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un tipo de sangre existente
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateBloodType(int id, [FromBody] TypeOfBlood bloodType)
        {
            try
            {
                var existingBloodType = await _bloodTypeRepository.GetByIdAsync(id);

                if (existingBloodType == null)
                {
                    return NotFound(new { message = "Tipo de sangre no encontrado" });
                }

                existingBloodType.Name = bloodType.Name;
                existingBloodType.IsActive = bloodType.IsActive;

                await _bloodTypeRepository.UpdateAsync(existingBloodType);
                return Ok(new { message = "Tipo de sangre actualizado con éxito", bloodType = existingBloodType });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el tipo de sangre", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un tipo de sangre
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteBloodType(int id)
        {
            try
            {
                var bloodType = await _bloodTypeRepository.GetByIdAsync(id);

                if (bloodType == null)
                {
                    return NotFound(new { message = "Tipo de sangre no encontrado" });
                }

                await _bloodTypeRepository.DeleteAsync(bloodType);
                return Ok(new { message = "Tipo de sangre eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el tipo de sangre", error = ex.Message });
            }
        }
    }
}
