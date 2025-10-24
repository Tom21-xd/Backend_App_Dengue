using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("Symptom")]
    [ApiController]
    public class SymptomControllerEF : ControllerBase
    {
        private readonly IRepository<Symptom> _symptomRepository;

        public SymptomControllerEF(IRepository<Symptom> symptomRepository)
        {
            _symptomRepository = symptomRepository;
        }

        /// <summary>
        /// Obtiene todos los síntomas
        /// </summary>
        [HttpGet]
        [Route("getSymptoms")]
        public async Task<IActionResult> GetSymptoms()
        {
            try
            {
                var symptoms = await _symptomRepository.GetAllAsync();
                return Ok(symptoms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener síntomas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un síntoma por ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetSymptomById(int id)
        {
            try
            {
                var symptom = await _symptomRepository.GetByIdAsync(id);

                if (symptom == null)
                {
                    return NotFound(new { message = "Síntoma no encontrado" });
                }

                return Ok(symptom);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el síntoma", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo síntoma
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSymptom([FromBody] Symptom symptom)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(symptom.Name))
                {
                    return BadRequest(new { message = "El nombre del síntoma es requerido" });
                }

                var createdSymptom = await _symptomRepository.AddAsync(symptom);
                return CreatedAtAction(nameof(GetSymptomById), new { id = createdSymptom.Id }, createdSymptom);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el síntoma", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un síntoma existente
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateSymptom(int id, [FromBody] Symptom symptom)
        {
            try
            {
                var existingSymptom = await _symptomRepository.GetByIdAsync(id);

                if (existingSymptom == null)
                {
                    return NotFound(new { message = "Síntoma no encontrado" });
                }

                existingSymptom.Name = symptom.Name;
                existingSymptom.IsActive = symptom.IsActive;

                await _symptomRepository.UpdateAsync(existingSymptom);
                return Ok(new { message = "Síntoma actualizado con éxito", symptom = existingSymptom });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el síntoma", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un síntoma
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteSymptom(int id)
        {
            try
            {
                var symptom = await _symptomRepository.GetByIdAsync(id);

                if (symptom == null)
                {
                    return NotFound(new { message = "Síntoma no encontrado" });
                }

                await _symptomRepository.DeleteAsync(symptom);
                return Ok(new { message = "Síntoma eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el síntoma", error = ex.Message });
            }
        }
    }
}
