using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("PatientState")]
    [ApiController]
    public class PatientStateControllerEF : ControllerBase
    {
        private readonly IRepository<PatientState> _patientStateRepository;

        public PatientStateControllerEF(IRepository<PatientState> patientStateRepository)
        {
            _patientStateRepository = patientStateRepository;
        }

        /// <summary>
        /// Get all patient states
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPatientStates()
        {
            try
            {
                var states = await _patientStateRepository.FindAsync(ps => ps.IsActive);
                return Ok(states);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estados de paciente", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient state by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientStateById(int id)
        {
            try
            {
                var state = await _patientStateRepository.GetByIdAsync(id);
                if (state == null || !state.IsActive)
                {
                    return NotFound(new { message = "Estado de paciente no encontrado" });
                }

                return Ok(state);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estado de paciente", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new patient state
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePatientState([FromBody] PatientState patientState)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                patientState.IsActive = true;
                var created = await _patientStateRepository.AddAsync(patientState);

                return CreatedAtAction(nameof(GetPatientStateById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear estado de paciente", error = ex.Message });
            }
        }

        /// <summary>
        /// Update patient state
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatientState(int id, [FromBody] PatientState patientState)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existing = await _patientStateRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = "Estado de paciente no encontrado" });
                }

                existing.Name = patientState.Name;
                existing.Description = patientState.Description;
                existing.SeverityLevel = patientState.SeverityLevel;
                existing.ColorIndicator = patientState.ColorIndicator;
                existing.IsActive = patientState.IsActive;

                await _patientStateRepository.UpdateAsync(existing);

                return Ok(existing);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar estado de paciente", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete patient state (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatientState(int id)
        {
            try
            {
                var state = await _patientStateRepository.GetByIdAsync(id);
                if (state == null)
                {
                    return NotFound(new { message = "Estado de paciente no encontrado" });
                }

                state.IsActive = false;
                await _patientStateRepository.UpdateAsync(state);

                return Ok(new { message = "Estado de paciente eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar estado de paciente", error = ex.Message });
            }
        }
    }
}
