using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    [Route("Dengue")]
    [ApiController]
    public class TypeOfDengueControllerEF : ControllerBase
    {
        private readonly IRepository<TypeOfDengue> _dengueTypeRepository;
        private readonly AppDbContext _context;

        public TypeOfDengueControllerEF(IRepository<TypeOfDengue> dengueTypeRepository, AppDbContext context)
        {
            _dengueTypeRepository = dengueTypeRepository;
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los tipos de dengue
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
        /// Obtiene un tipo de dengue por ID
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
        /// Crea un nuevo tipo de dengue
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
        /// Actualiza un tipo de dengue existente
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
        /// Elimina un tipo de dengue
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

        /// <summary>
        /// Obtiene todos los síntomas para un tipo de dengue específico
        /// </summary>
        [HttpGet]
        [Route("{typeOfDengueId}/symptoms")]
        public async Task<IActionResult> GetSymptomsByDengueType(int typeOfDengueId)
        {
            try
            {
                var dengueType = await _dengueTypeRepository.GetByIdAsync(typeOfDengueId);

                if (dengueType == null)
                {
                    return NotFound(new { message = "Tipo de dengue no encontrado" });
                }

                var symptoms = await _context.TypeOfDengueSymptoms
                    .Where(tds => tds.TypeOfDengueId == typeOfDengueId)
                    .Include(tds => tds.Symptom)
                    .Select(tds => new
                    {
                        tds.Symptom.Id,
                        tds.Symptom.Name,
                        tds.Symptom.IsActive
                    })
                    .ToListAsync();

                return Ok(new
                {
                    typeOfDengueId = typeOfDengueId,
                    typeOfDengueName = dengueType.Name,
                    symptoms = symptoms
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener síntomas del tipo de dengue", error = ex.Message });
            }
        }

        /// <summary>
        /// Asocia un síntoma con un tipo de dengue
        /// </summary>
        [HttpPost]
        [Route("{typeOfDengueId}/symptoms/{symptomId}")]
        public async Task<IActionResult> AssociateSymptomWithDengueType(int typeOfDengueId, int symptomId)
        {
            try
            {
                var dengueType = await _dengueTypeRepository.GetByIdAsync(typeOfDengueId);
                if (dengueType == null)
                {
                    return NotFound(new { message = "Tipo de dengue no encontrado" });
                }

                var symptom = await _context.Symptoms.FindAsync(symptomId);
                if (symptom == null)
                {
                    return NotFound(new { message = "Síntoma no encontrado" });
                }

                // Verificar si la relación ya existe
                var existingRelation = await _context.TypeOfDengueSymptoms
                    .FirstOrDefaultAsync(tds => tds.TypeOfDengueId == typeOfDengueId && tds.SymptomId == symptomId);

                if (existingRelation != null)
                {
                    return BadRequest(new { message = "La relación entre el tipo de dengue y el síntoma ya existe" });
                }

                var newRelation = new TypeOfDengueSymptom
                {
                    TypeOfDengueId = typeOfDengueId,
                    SymptomId = symptomId
                };

                _context.TypeOfDengueSymptoms.Add(newRelation);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Síntoma asociado con éxito al tipo de dengue",
                    relationId = newRelation.Id,
                    typeOfDengueId = typeOfDengueId,
                    symptomId = symptomId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al asociar síntoma con tipo de dengue", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina la asociación de un síntoma de un tipo de dengue
        /// </summary>
        [HttpDelete]
        [Route("{typeOfDengueId}/symptoms/{symptomId}")]
        public async Task<IActionResult> RemoveSymptomFromDengueType(int typeOfDengueId, int symptomId)
        {
            try
            {
                var relation = await _context.TypeOfDengueSymptoms
                    .FirstOrDefaultAsync(tds => tds.TypeOfDengueId == typeOfDengueId && tds.SymptomId == symptomId);

                if (relation == null)
                {
                    return NotFound(new { message = "La relación entre el tipo de dengue y el síntoma no existe" });
                }

                _context.TypeOfDengueSymptoms.Remove(relation);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Síntoma desasociado con éxito del tipo de dengue" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al desasociar síntoma del tipo de dengue", error = ex.Message });
            }
        }

        /// <summary>
        /// Diagnostica el tipo de dengue basado en síntomas
        /// </summary>
        [HttpPost]
        [Route("diagnose")]
        public async Task<IActionResult> DiagnoseDengueType([FromBody] List<int> symptomIds)
        {
            try
            {
                if (symptomIds == null || symptomIds.Count == 0)
                {
                    return BadRequest(new { message = "Debe proporcionar al menos un síntoma" });
                }

                // Obtener todos los tipos de dengue con sus síntomas
                var dengueTypesWithSymptoms = await _context.TypesOfDengue
                    .Include(td => td.TypeOfDengueSymptoms)
                    .ThenInclude(tds => tds.Symptom)
                    .Where(td => td.IsActive)
                    .ToListAsync();

                if (!dengueTypesWithSymptoms.Any())
                {
                    return NotFound(new { message = "No hay tipos de dengue configurados" });
                }

                // Calcular porcentaje de coincidencia para cada tipo de dengue
                var results = dengueTypesWithSymptoms.Select(dengueType =>
                {
                    var dengueSymptomIds = dengueType.TypeOfDengueSymptoms
                        .Select(tds => tds.SymptomId)
                        .ToList();

                    // Contar cuántos síntomas de entrada coinciden con este tipo de dengue
                    var matchingSymptoms = symptomIds.Intersect(dengueSymptomIds).Count();

                    // Calcular porcentaje de coincidencia basado en:
                    // 1. Cuántos de los síntomas del paciente coinciden (precisión)
                    // 2. Cuántos de los síntomas del tipo de dengue están presentes (exhaustividad)
                    var precisionPercentage = symptomIds.Count > 0
                        ? (double)matchingSymptoms / symptomIds.Count * 100
                        : 0;

                    var recallPercentage = dengueSymptomIds.Count > 0
                        ? (double)matchingSymptoms / dengueSymptomIds.Count * 100
                        : 0;

                    // F1 Score (media armónica de precisión y exhaustividad)
                    var matchPercentage = (precisionPercentage + recallPercentage) > 0
                        ? 2 * (precisionPercentage * recallPercentage) / (precisionPercentage + recallPercentage)
                        : 0;

                    return new
                    {
                        typeOfDengueId = dengueType.Id,
                        typeOfDengueName = dengueType.Name,
                        matchingSymptoms = matchingSymptoms,
                        totalSymptomsInType = dengueSymptomIds.Count,
                        totalSymptomsProvided = symptomIds.Count,
                        matchPercentage = Math.Round(matchPercentage, 2),
                        precisionPercentage = Math.Round(precisionPercentage, 2),
                        recallPercentage = Math.Round(recallPercentage, 2)
                    };
                })
                .OrderByDescending(r => r.matchPercentage)
                .ToList();

                var bestMatch = results.First();

                return Ok(new
                {
                    mostLikelyDiagnosis = new
                    {
                        typeOfDengueId = bestMatch.typeOfDengueId,
                        typeOfDengueName = bestMatch.typeOfDengueName,
                        matchPercentage = bestMatch.matchPercentage,
                        confidence = bestMatch.matchPercentage >= 70 ? "Alta" :
                                    bestMatch.matchPercentage >= 50 ? "Media" : "Baja"
                    },
                    allResults = results,
                    disclaimer = "Este diagnóstico es orientativo. Consulte con un profesional de la salud para un diagnóstico definitivo."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al diagnosticar tipo de dengue", error = ex.Message });
            }
        }
    }
}
