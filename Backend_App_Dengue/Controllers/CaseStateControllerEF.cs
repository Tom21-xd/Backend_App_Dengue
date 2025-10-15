using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("CaseState")]
    [ApiController]
    public class CaseStateControllerEF : ControllerBase
    {
        private readonly IRepository<CaseState> _caseStateRepository;

        public CaseStateControllerEF(IRepository<CaseState> caseStateRepository)
        {
            _caseStateRepository = caseStateRepository;
        }

        /// <summary>
        /// Get all case states
        /// </summary>
        [HttpGet]
        [Route("getCaseStates")]
        public async Task<IActionResult> GetCaseStates()
        {
            try
            {
                var caseStates = await _caseStateRepository.GetAllAsync();
                return Ok(caseStates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estados de caso", error = ex.Message });
            }
        }

        /// <summary>
        /// Get case state by ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetCaseStateById(int id)
        {
            try
            {
                var caseState = await _caseStateRepository.GetByIdAsync(id);

                if (caseState == null)
                {
                    return NotFound(new { message = "Estado de caso no encontrado" });
                }

                return Ok(caseState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el estado de caso", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new case state
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCaseState([FromBody] CaseState caseState)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(caseState.Name))
                {
                    return BadRequest(new { message = "El nombre del estado de caso es requerido" });
                }

                var createdCaseState = await _caseStateRepository.AddAsync(caseState);
                return CreatedAtAction(nameof(GetCaseStateById), new { id = createdCaseState.Id }, createdCaseState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el estado de caso", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing case state
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateCaseState(int id, [FromBody] CaseState caseState)
        {
            try
            {
                var existingCaseState = await _caseStateRepository.GetByIdAsync(id);

                if (existingCaseState == null)
                {
                    return NotFound(new { message = "Estado de caso no encontrado" });
                }

                existingCaseState.Name = caseState.Name;
                existingCaseState.IsActive = caseState.IsActive;

                await _caseStateRepository.UpdateAsync(existingCaseState);
                return Ok(new { message = "Estado de caso actualizado con éxito", caseState = existingCaseState });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el estado de caso", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a case state
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteCaseState(int id)
        {
            try
            {
                var caseState = await _caseStateRepository.GetByIdAsync(id);

                if (caseState == null)
                {
                    return NotFound(new { message = "Estado de caso no encontrado" });
                }

                await _caseStateRepository.DeleteAsync(caseState);
                return Ok(new { message = "Estado de caso eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el estado de caso", error = ex.Message });
            }
        }
    }
}
