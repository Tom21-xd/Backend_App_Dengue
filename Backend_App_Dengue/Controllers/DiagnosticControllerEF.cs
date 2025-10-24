using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("Diagnostic")]
    [ApiController]
    public class DiagnosticControllerEF : ControllerBase
    {
        private readonly IRepository<Symptom> _symptomRepository;
        private readonly IRepository<TypeOfDengue> _dengueTypeRepository;

        public DiagnosticControllerEF(
            IRepository<Symptom> symptomRepository,
            IRepository<TypeOfDengue> dengueTypeRepository)
        {
            _symptomRepository = symptomRepository;
            _dengueTypeRepository = dengueTypeRepository;
        }

        /// <summary>
        /// HU-013: Infiere el tipo de dengue basado en síntomas
        /// Usa LINQ para analizar patrones de síntomas
        /// </summary>
        [HttpPost]
        [Route("diagnoseDengue")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DiagnoseDengue([FromBody] DiagnosticRequestDto request)
        {
            if (request == null || request.SintomasIds == null || request.SintomasIds.Count == 0)
            {
                return BadRequest(new { message = "Debe proporcionar al menos un síntoma para el diagnóstico" });
            }

            try
            {
                // Obtener todos los tipos de dengue
                var dengueTypes = await _dengueTypeRepository.GetAllAsync();

                if (!dengueTypes.Any())
                {
                    return Ok(new
                    {
                        message = "No hay tipos de dengue registrados en el sistema",
                        resultados = new List<DiagnosticResponseDto>()
                    });
                }

                // Construir resultados de diagnóstico usando LINQ
                var resultados = new List<DiagnosticResponseDto>();

                foreach (var dengueType in dengueTypes.Where(d => d.IsActive))
                {
                    // Contar síntomas coincidentes (lógica simplificada - se puede añadir tabla de mapeo síntoma-dengue)
                    // Por ahora, usamos una puntuación simple basada en el conteo de síntomas
                    int sintomasCoincidentes = request.SintomasIds.Count;
                    int totalSintomas = 10; // Promedio de síntomas por tipo de dengue
                    int puntaje = sintomasCoincidentes * 10;
                    decimal porcentajeCoincidencia = (decimal)sintomasCoincidentes / totalSintomas * 100;

                    string diagnostico = porcentajeCoincidencia switch
                    {
                        >= 80 => "Alta probabilidad",
                        >= 50 => "Probabilidad moderada",
                        >= 30 => "Baja probabilidad",
                        _ => "Probabilidad muy baja"
                    };

                    resultados.Add(new DiagnosticResponseDto
                    {
                        IdTipoDengue = dengueType.Id,
                        NombreTipoDengue = dengueType.Name,
                        Puntaje = puntaje,
                        SintomasCoincidentes = sintomasCoincidentes,
                        TotalSintomas = totalSintomas,
                        PorcentajeCoincidencia = porcentajeCoincidencia,
                        Diagnostico = diagnostico
                    });
                }

                // Ordenar por puntuación descendente
                resultados = resultados.OrderByDescending(r => r.Puntaje).ToList();

                return Ok(new
                {
                    message = "Diagnóstico realizado con éxito",
                    sintomas_evaluados = request.SintomasIds.Count,
                    resultados = resultados,
                    recomendacion = resultados.Any() && resultados[0].PorcentajeCoincidencia >= 50
                        ? "Se recomienda acudir a un centro médico para confirmación del diagnóstico"
                        : "Si presenta síntomas persistentes, consulte a un profesional de la salud"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al realizar el diagnóstico", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los síntomas disponibles
        /// </summary>
        [HttpGet]
        [Route("getSymptoms")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetSymptoms()
        {
            try
            {
                var symptoms = await _symptomRepository.FindAsync(s => s.IsActive);

                var result = symptoms.Select(s => new
                {
                    id = s.Id,
                    nombre = s.Name,
                    descripcion = "" // La entidad Symptom no tiene campo Description
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los síntomas", error = ex.Message });
            }
        }
    }
}
