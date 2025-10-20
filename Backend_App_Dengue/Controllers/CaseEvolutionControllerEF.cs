using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend_App_Dengue.Controllers
{
    [Route("api")]
    [ApiController]
    public class CaseEvolutionControllerEF : ControllerBase
    {
        private readonly IRepository<CaseEvolution> _evolutionRepository;
        private readonly IRepository<Case> _caseRepository;
        private readonly IRepository<PatientState> _patientStateRepository;
        private readonly IRepository<TypeOfDengue> _typeOfDengueRepository;
        private readonly AppDbContext _context;

        public CaseEvolutionControllerEF(
            IRepository<CaseEvolution> evolutionRepository,
            IRepository<Case> caseRepository,
            IRepository<PatientState> patientStateRepository,
            IRepository<TypeOfDengue> typeOfDengueRepository,
            AppDbContext context)
        {
            _evolutionRepository = evolutionRepository;
            _caseRepository = caseRepository;
            _patientStateRepository = patientStateRepository;
            _typeOfDengueRepository = typeOfDengueRepository;
            _context = context;
        }

        #region Evolution CRUD

        /// <summary>
        /// Create new evolution for a case
        /// </summary>
        [HttpPost("Case/{caseId}/evolution")]
        public async Task<IActionResult> CreateEvolution(int caseId, [FromBody] CaseEvolution evolutionData)
        {
            try
            {
                // Validate case exists
                var caseEntity = await _caseRepository.GetByIdAsync(caseId);
                if (caseEntity == null || !caseEntity.IsActive)
                {
                    return NotFound(new { message = "Caso no encontrado" });
                }

                // Validate patient state exists
                var patientState = await _patientStateRepository.GetByIdAsync(evolutionData.PatientStateId);
                if (patientState == null || !patientState.IsActive)
                {
                    return BadRequest(new { message = "Estado de paciente no válido" });
                }

                // Validate type of dengue exists
                var typeOfDengue = await _typeOfDengueRepository.GetByIdAsync(evolutionData.TypeOfDengueId);
                if (typeOfDengue == null || !typeOfDengue.IsActive)
                {
                    return BadRequest(new { message = "Tipo de dengue no válido" });
                }

                // Set case ID and timestamps
                evolutionData.CaseId = caseId;
                evolutionData.CreatedAt = DateTime.Now;
                evolutionData.IsActive = true;

                // Check if dengue type changed from previous evolution
                var lastEvolution = await _context.CaseEvolutions
                    .Where(e => e.CaseId == caseId && e.IsActive)
                    .OrderByDescending(e => e.EvolutionDate)
                    .FirstOrDefaultAsync();

                if (lastEvolution != null)
                {
                    evolutionData.DengueTypeChanged = lastEvolution.TypeOfDengueId != evolutionData.TypeOfDengueId;

                    // Detect deterioration
                    if (evolutionData.PatientStateId > lastEvolution.PatientStateId ||
                        (evolutionData.Platelets.HasValue && lastEvolution.Platelets.HasValue &&
                         evolutionData.Platelets < lastEvolution.Platelets) ||
                        (evolutionData.Hematocrit.HasValue && lastEvolution.Hematocrit.HasValue &&
                         evolutionData.Hematocrit > lastEvolution.Hematocrit + 5))
                    {
                        evolutionData.DeteriorationDetected = true;
                    }
                }

                // Create evolution
                var created = await _evolutionRepository.AddAsync(evolutionData);

                // Update case with latest evolution info
                caseEntity.CurrentPatientStateId = created.PatientStateId;
                caseEntity.LastEvolutionId = created.Id;
                caseEntity.CurrentTypeOfDengueId = created.TypeOfDengueId;
                await _caseRepository.UpdateAsync(caseEntity);

                // Load navigation properties for response
                var result = await _context.CaseEvolutions
                    .Include(e => e.PatientState)
                    .Include(e => e.Doctor)
                    .Include(e => e.TypeOfDengue)
                    .FirstOrDefaultAsync(e => e.Id == created.Id);

                return CreatedAtAction(nameof(GetEvolutionById), new { id = created.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear evolución", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all evolutions for a case
        /// </summary>
        [HttpGet("Case/{caseId}/evolution")]
        public async Task<IActionResult> GetCaseEvolutions(int caseId)
        {
            try
            {
                var evolutions = await _context.CaseEvolutions
                    .Include(e => e.PatientState)
                    .Include(e => e.Doctor)
                    .Include(e => e.TypeOfDengue)
                    .Where(e => e.CaseId == caseId && e.IsActive)
                    .OrderByDescending(e => e.EvolutionDate)
                    .ToListAsync();

                return Ok(evolutions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener evoluciones", error = ex.Message });
            }
        }

        /// <summary>
        /// Get latest evolution for a case
        /// </summary>
        [HttpGet("Case/{caseId}/evolution/latest")]
        public async Task<IActionResult> GetLatestEvolution(int caseId)
        {
            try
            {
                var evolution = await _context.CaseEvolutions
                    .Include(e => e.PatientState)
                    .Include(e => e.Doctor)
                    .Include(e => e.TypeOfDengue)
                    .Where(e => e.CaseId == caseId && e.IsActive)
                    .OrderByDescending(e => e.EvolutionDate)
                    .FirstOrDefaultAsync();

                if (evolution == null)
                {
                    return NotFound(new { message = "No se encontraron evoluciones para este caso" });
                }

                return Ok(evolution);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener última evolución", error = ex.Message });
            }
        }

        /// <summary>
        /// Get evolution by ID
        /// </summary>
        [HttpGet("Evolution/{id}")]
        public async Task<IActionResult> GetEvolutionById(int id)
        {
            try
            {
                var evolution = await _context.CaseEvolutions
                    .Include(e => e.PatientState)
                    .Include(e => e.Doctor)
                    .Include(e => e.TypeOfDengue)
                    .Include(e => e.Case)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (evolution == null || !evolution.IsActive)
                {
                    return NotFound(new { message = "Evolución no encontrada" });
                }

                return Ok(evolution);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener evolución", error = ex.Message });
            }
        }

        /// <summary>
        /// Update evolution
        /// </summary>
        [HttpPut("Evolution/{id}")]
        public async Task<IActionResult> UpdateEvolution(int id, [FromBody] CaseEvolution evolutionData)
        {
            try
            {
                var existing = await _evolutionRepository.GetByIdAsync(id);
                if (existing == null || !existing.IsActive)
                {
                    return NotFound(new { message = "Evolución no encontrada" });
                }

                // Update fields
                existing.PatientStateId = evolutionData.PatientStateId;
                existing.TypeOfDengueId = evolutionData.TypeOfDengueId;
                existing.DayOfIllness = evolutionData.DayOfIllness;
                existing.ReportedSymptomsJson = evolutionData.ReportedSymptomsJson;
                existing.Temperature = evolutionData.Temperature;
                existing.SystolicBloodPressure = evolutionData.SystolicBloodPressure;
                existing.DiastolicBloodPressure = evolutionData.DiastolicBloodPressure;
                existing.HeartRate = evolutionData.HeartRate;
                existing.RespiratoryRate = evolutionData.RespiratoryRate;
                existing.OxygenSaturation = evolutionData.OxygenSaturation;
                existing.Platelets = evolutionData.Platelets;
                existing.Hematocrit = evolutionData.Hematocrit;
                existing.WhiteBloodCells = evolutionData.WhiteBloodCells;
                existing.Hemoglobin = evolutionData.Hemoglobin;
                existing.AST = evolutionData.AST;
                existing.ALT = evolutionData.ALT;
                existing.ClinicalObservations = evolutionData.ClinicalObservations;
                existing.PrescribedTreatment = evolutionData.PrescribedTreatment;
                existing.RequestedTests = evolutionData.RequestedTests;
                existing.DengueTypeChanged = evolutionData.DengueTypeChanged;
                existing.DeteriorationDetected = evolutionData.DeteriorationDetected;
                existing.RequiresHospitalization = evolutionData.RequiresHospitalization;
                existing.RequiresICU = evolutionData.RequiresICU;
                existing.NextAppointment = evolutionData.NextAppointment;
                existing.PatientRecommendations = evolutionData.PatientRecommendations;
                existing.UpdatedAt = DateTime.Now;

                await _evolutionRepository.UpdateAsync(existing);

                // Load navigation properties for response
                var result = await _context.CaseEvolutions
                    .Include(e => e.PatientState)
                    .Include(e => e.Doctor)
                    .Include(e => e.TypeOfDengue)
                    .FirstOrDefaultAsync(e => e.Id == id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar evolución", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete evolution (soft delete)
        /// </summary>
        [HttpDelete("Evolution/{id}")]
        public async Task<IActionResult> DeleteEvolution(int id)
        {
            try
            {
                var evolution = await _evolutionRepository.GetByIdAsync(id);
                if (evolution == null)
                {
                    return NotFound(new { message = "Evolución no encontrada" });
                }

                evolution.IsActive = false;
                evolution.UpdatedAt = DateTime.Now;
                await _evolutionRepository.UpdateAsync(evolution);

                return Ok(new { message = "Evolución eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar evolución", error = ex.Message });
            }
        }

        #endregion

        #region Summary and Trends

        /// <summary>
        /// Get evolution summary with trends for a case
        /// </summary>
        [HttpGet("Case/{caseId}/evolution/summary")]
        public async Task<IActionResult> GetEvolutionSummary(int caseId)
        {
            try
            {
                var evolutions = await _context.CaseEvolutions
                    .Include(e => e.PatientState)
                    .Include(e => e.Doctor)
                    .Include(e => e.TypeOfDengue)
                    .Where(e => e.CaseId == caseId && e.IsActive)
                    .OrderBy(e => e.EvolutionDate)
                    .ToListAsync();

                if (!evolutions.Any())
                {
                    return NotFound(new { message = "No se encontraron evoluciones para este caso" });
                }

                var latest = evolutions.Last();
                var caseEntity = await _caseRepository.GetByIdAsync(caseId);

                // Calculate trends
                string plateletsTrend = "estable";
                string hematocritTrend = "estable";

                if (evolutions.Count > 1)
                {
                    var previousEvolutions = evolutions.TakeLast(3).ToList();

                    // Platelets trend
                    var plateletsValues = previousEvolutions
                        .Where(e => e.Platelets.HasValue)
                        .Select(e => e.Platelets!.Value)
                        .ToList();

                    if (plateletsValues.Count >= 2)
                    {
                        var trend = plateletsValues.Last() - plateletsValues.First();
                        if (trend > 20000) plateletsTrend = "mejorando";
                        else if (trend < -20000) plateletsTrend = "empeorando";
                    }

                    // Hematocrit trend
                    var hematocritValues = previousEvolutions
                        .Where(e => e.Hematocrit.HasValue)
                        .Select(e => e.Hematocrit!.Value)
                        .ToList();

                    if (hematocritValues.Count >= 2)
                    {
                        var trend = hematocritValues.Last() - hematocritValues.First();
                        if (trend > 5) hematocritTrend = "empeorando";
                        else if (trend < -5) hematocritTrend = "mejorando";
                    }
                }

                var summary = new
                {
                    ID_CASO = caseId,
                    ESTADO_ACTUAL = latest.PatientState,
                    DIA_ENFERMEDAD_ACTUAL = latest.DayOfIllness ?? 0,
                    TOTAL_EVOLUCIONES = evolutions.Count,
                    ULTIMA_EVOLUCION = latest,
                    TENDENCIA_PLAQUETAS = plateletsTrend,
                    TENDENCIA_HEMATOCRITO = hematocritTrend,
                    TIENE_SIGNOS_ALARMA = latest.DeteriorationDetected || latest.RequiresHospitalization || latest.RequiresICU,
                    EVOLUCIONES = evolutions
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener resumen de evolución", error = ex.Message });
            }
        }

        /// <summary>
        /// Get evolutions by day of illness
        /// </summary>
        [HttpGet("Case/{caseId}/evolution/day/{day}")]
        public async Task<IActionResult> GetEvolutionsByDay(int caseId, int day)
        {
            try
            {
                var evolutions = await _context.CaseEvolutions
                    .Include(e => e.PatientState)
                    .Include(e => e.Doctor)
                    .Include(e => e.TypeOfDengue)
                    .Where(e => e.CaseId == caseId && e.DayOfIllness == day && e.IsActive)
                    .OrderByDescending(e => e.EvolutionDate)
                    .ToListAsync();

                return Ok(evolutions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener evoluciones por día", error = ex.Message });
            }
        }

        /// <summary>
        /// Get labs history for charts
        /// </summary>
        [HttpGet("Case/{caseId}/labs-history")]
        public async Task<IActionResult> GetLabsHistory(int caseId)
        {
            try
            {
                var evolutions = await _context.CaseEvolutions
                    .Where(e => e.CaseId == caseId && e.IsActive)
                    .OrderBy(e => e.EvolutionDate)
                    .Select(e => new
                    {
                        fecha = e.EvolutionDate,
                        dia = e.DayOfIllness,
                        plaquetas = e.Platelets,
                        hematocrito = e.Hematocrit,
                        hemoglobina = e.Hemoglobin,
                        leucocitos = e.WhiteBloodCells,
                        ast = e.AST,
                        alt = e.ALT
                    })
                    .ToListAsync();

                var history = new Dictionary<string, List<object>>
                {
                    { "plaquetas", evolutions.Where(e => e.plaquetas.HasValue).Cast<object>().ToList() },
                    { "hematocrito", evolutions.Where(e => e.hematocrito.HasValue).Cast<object>().ToList() },
                    { "hemoglobina", evolutions.Where(e => e.hemoglobina.HasValue).Cast<object>().ToList() },
                    { "leucocitos", evolutions.Where(e => e.leucocitos.HasValue).Cast<object>().ToList() },
                    { "transaminasas", evolutions.Where(e => e.ast.HasValue || e.alt.HasValue).Cast<object>().ToList() }
                };

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener historial de laboratorios", error = ex.Message });
            }
        }

        /// <summary>
        /// Get vital signs history
        /// </summary>
        [HttpGet("Case/{caseId}/vital-signs-history")]
        public async Task<IActionResult> GetVitalSignsHistory(int caseId)
        {
            try
            {
                var evolutions = await _context.CaseEvolutions
                    .Where(e => e.CaseId == caseId && e.IsActive)
                    .OrderBy(e => e.EvolutionDate)
                    .Select(e => new
                    {
                        fecha = e.EvolutionDate,
                        dia = e.DayOfIllness,
                        temperatura = e.Temperature,
                        presion_sistolica = e.SystolicBloodPressure,
                        presion_diastolica = e.DiastolicBloodPressure,
                        frecuencia_cardiaca = e.HeartRate,
                        frecuencia_respiratoria = e.RespiratoryRate,
                        saturacion_oxigeno = e.OxygenSaturation
                    })
                    .ToListAsync();

                var history = new Dictionary<string, List<object>>
                {
                    { "temperatura", evolutions.Where(e => e.temperatura.HasValue).Cast<object>().ToList() },
                    { "presion_arterial", evolutions.Where(e => e.presion_sistolica.HasValue).Cast<object>().ToList() },
                    { "frecuencia_cardiaca", evolutions.Where(e => e.frecuencia_cardiaca.HasValue).Cast<object>().ToList() },
                    { "frecuencia_respiratoria", evolutions.Where(e => e.frecuencia_respiratoria.HasValue).Cast<object>().ToList() },
                    { "saturacion_oxigeno", evolutions.Where(e => e.saturacion_oxigeno.HasValue).Cast<object>().ToList() }
                };

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener historial de signos vitales", error = ex.Message });
            }
        }

        #endregion

        #region Warning Signs and Urgent Cases

        /// <summary>
        /// Get cases with warning signs
        /// </summary>
        [HttpGet("Evolution/warning-signs")]
        public async Task<IActionResult> GetCasesWithWarningSigns()
        {
            try
            {
                var evolutions = await _context.CaseEvolutions
                    .Include(e => e.Case)
                    .Include(e => e.PatientState)
                    .Include(e => e.Doctor)
                    .Where(e => e.IsActive &&
                               (e.DeteriorationDetected ||
                                e.RequiresHospitalization ||
                                (e.Platelets.HasValue && e.Platelets < 50000) ||
                                (e.Hematocrit.HasValue && e.Hematocrit > 45)))
                    .OrderByDescending(e => e.EvolutionDate)
                    .ToListAsync();

                return Ok(evolutions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener casos con signos de alarma", error = ex.Message });
            }
        }

        /// <summary>
        /// Get urgent cases (require immediate attention)
        /// </summary>
        [HttpGet("Evolution/urgent")]
        public async Task<IActionResult> GetUrgentCases()
        {
            try
            {
                var evolutions = await _context.CaseEvolutions
                    .Include(e => e.Case)
                    .Include(e => e.PatientState)
                    .Include(e => e.Doctor)
                    .Where(e => e.IsActive && (e.RequiresICU ||
                               (e.Platelets.HasValue && e.Platelets < 20000) ||
                               (e.SystolicBloodPressure.HasValue && e.SystolicBloodPressure < 90)))
                    .OrderByDescending(e => e.EvolutionDate)
                    .ToListAsync();

                return Ok(evolutions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener casos urgentes", error = ex.Message });
            }
        }

        /// <summary>
        /// Get evolutions by patient state
        /// </summary>
        [HttpGet("Evolution/by-state/{stateId}")]
        public async Task<IActionResult> GetEvolutionsByState(int stateId)
        {
            try
            {
                var evolutions = await _context.CaseEvolutions
                    .Include(e => e.Case)
                    .Include(e => e.PatientState)
                    .Include(e => e.Doctor)
                    .Where(e => e.PatientStateId == stateId && e.IsActive)
                    .OrderByDescending(e => e.EvolutionDate)
                    .ToListAsync();

                return Ok(evolutions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener evoluciones por estado", error = ex.Message });
            }
        }

        #endregion
    }
}
