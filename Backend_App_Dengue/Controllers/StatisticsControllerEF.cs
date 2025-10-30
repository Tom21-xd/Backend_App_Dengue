using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("Statistics")]
    [ApiController]
    public class StatisticsControllerEF : ControllerBase
    {
        private readonly IRepository<Case> _caseRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Hospital> _hospitalRepository;
        private readonly IRepository<Publication> _publicationRepository;
        private readonly IRepository<Notification> _notificationRepository;

        public StatisticsControllerEF(
            IRepository<Case> caseRepository,
            IRepository<User> userRepository,
            IRepository<Hospital> hospitalRepository,
            IRepository<Publication> publicationRepository,
            IRepository<Notification> notificationRepository)
        {
            _caseRepository = caseRepository;
            _userRepository = userRepository;
            _hospitalRepository = hospitalRepository;
            _publicationRepository = publicationRepository;
            _notificationRepository = notificationRepository;
        }

        /// <summary>
        /// HU-008: Estadísticas generales usando agregaciones LINQ
        /// </summary>
        [HttpGet]
        [Route("general")]
        [ProducesResponseType(typeof(GeneralStatsModel), 200)]
        public async Task<IActionResult> GetGeneralStatistics()
        {
            try
            {
                var allCases = await _caseRepository.GetAllAsync();
                var activeCases = allCases.Where(c => c.IsActive);

                var stats = new GeneralStatsModel
                {
                    TotalCasos = allCases.Count(),
                    CasosActivos = activeCases.Where(c => c.StateId != 3).Count(), // 3 = Finalizado
                    CasosFinalizados = activeCases.Where(c => c.StateId == 3).Count(),
                    CasosFallecidos = activeCases.Where(c => c.StateId == 4).Count(), // 4 = Fallecido
                    UsuariosEnfermos = activeCases.Select(c => c.PatientId).Distinct().Count(),
                    HospitalesActivos = await _hospitalRepository.CountAsync(h => h.IsActive),
                    TotalPublicaciones = await _publicationRepository.CountAsync(p => p.IsActive),
                    NotificacionesPendientes = await _notificationRepository.CountAsync(n => !n.IsRead && n.IsActive)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las estadísticas generales", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-008: Casos por tipo de dengue usando LINQ GroupBy
        /// </summary>
        [HttpGet]
        [Route("byDengueType")]
        [ProducesResponseType(typeof(List<DengueTypeStatsModel>), 200)]
        public async Task<IActionResult> GetCasesByDengueType()
        {
            try
            {
                var cases = await _caseRepository.GetAllAsync();
                var activeCases = cases.Where(c => c.IsActive);

                var stats = activeCases
                    .GroupBy(c => new { c.TypeOfDengueId, c.TypeOfDengue.Name })
                    .Select(g => new DengueTypeStatsModel
                    {
                        IdTipoDengue = g.Key.TypeOfDengueId,
                        NombreTipoDengue = g.Key.Name ?? "Sin especificar",
                        TotalCasos = g.Count(),
                        CasosActivos = g.Count(c => c.StateId != 3 && c.StateId != 4),
                        CasosFinalizados = g.Count(c => c.StateId == 3),
                        CasosFallecidos = g.Count(c => c.StateId == 4),
                        TasaMortalidad = g.Count() > 0
                            ? Math.Round((decimal)g.Count(c => c.StateId == 4) / g.Count() * 100, 2)
                            : 0
                    })
                    .OrderByDescending(s => s.TotalCasos)
                    .ToList();

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas por tipo de dengue", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-008: Cases by month using LINQ with date grouping
        /// </summary>
        [HttpGet]
        [Route("byMonth")]
        [ProducesResponseType(typeof(List<MonthlyStatsModel>), 200)]
        public async Task<IActionResult> GetCasesByMonth([FromQuery] int? year)
        {
            try
            {
                var targetYear = year ?? DateTime.Now.Year;
                var cases = await _caseRepository.GetAllAsync();

                var monthlyCases = cases
                    .Where(c => c.CreatedAt.Year == targetYear && c.IsActive)
                    .GroupBy(c => c.CreatedAt.Month)
                    .Select(g => new MonthlyStatsModel
                    {
                        Mes = g.Key,
                        NombreMes = new DateTime(targetYear, g.Key, 1).ToString("MMMM"),
                        TotalCasos = g.Count(),
                        SinAlarma = g.Count(c => c.TypeOfDengueId == 1), // Adjust IDs as needed
                        ConAlarma = g.Count(c => c.TypeOfDengueId == 2),
                        Grave = g.Count(c => c.TypeOfDengueId == 3)
                    })
                    .OrderBy(m => m.Mes)
                    .ToList();

                return Ok(monthlyCases);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas mensuales", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-008: Cases by department using LINQ navigation properties
        /// </summary>
        [HttpGet]
        [Route("byDepartment")]
        [ProducesResponseType(typeof(List<DepartmentStatsModel>), 200)]
        public async Task<IActionResult> GetCasesByDepartment()
        {
            try
            {
                var cases = await _caseRepository.GetAllAsync();
                var activeCases = cases.Where(c => c.IsActive);

                var stats = activeCases
                    .GroupBy(c => new
                    {
                        DepartmentId = c.Hospital.City.DepartmentId,
                        DepartmentName = c.Hospital.City.Department.Name
                    })
                    .Select(g => new DepartmentStatsModel
                    {
                        IdDepartamento = g.Key.DepartmentId,
                        NombreDepartamento = g.Key.DepartmentName ?? "Desconocido",
                        TotalCasos = g.Count(),
                        CasosActivos = g.Count(c => c.StateId != 3 && c.StateId != 4),
                        HospitalesInvolucrados = g.Select(c => c.HospitalId).Distinct().Count()
                    })
                    .OrderByDescending(s => s.TotalCasos)
                    .ToList();

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas por departamento", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-008: Case trends over time using LINQ temporal analysis
        /// </summary>
        [HttpGet]
        [Route("trends")]
        [ProducesResponseType(typeof(List<TrendStatsModel>), 200)]
        public async Task<IActionResult> GetCaseTrends([FromQuery] int? months)
        {
            try
            {
                var monthsToAnalyze = months ?? 6;
                var startDate = DateTime.Now.AddMonths(-monthsToAnalyze);

                var cases = await _caseRepository.GetAllAsync();
                var recentCases = cases.Where(c => c.CreatedAt >= startDate && c.IsActive);

                var trends = recentCases
                    .GroupBy(c => new { Year = c.CreatedAt.Year, Month = c.CreatedAt.Month })
                    .Select(g => new TrendStatsModel
                    {
                        Periodo = $"{g.Key.Year}-{g.Key.Month:D2}",
                        TotalCasos = g.Count(),
                        TasaMortalidad = g.Count() > 0
                            ? Math.Round((decimal)g.Count(c => c.StateId == 4) / g.Count() * 100, 2)
                            : 0
                    })
                    .OrderBy(t => t.Periodo)
                    .ToList();

                return Ok(trends);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener tendencias de casos", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-008: Top hospitals by case count using LINQ ordering
        /// </summary>
        [HttpGet]
        [Route("topHospitals")]
        [ProducesResponseType(typeof(List<TopHospitalModel>), 200)]
        public async Task<IActionResult> GetTopHospitals([FromQuery] int? limit)
        {
            try
            {
                var topLimit = limit ?? 10;
                var cases = await _caseRepository.GetAllAsync();
                var activeCases = cases.Where(c => c.IsActive);

                var topHospitals = activeCases
                    .GroupBy(c => new
                    {
                        c.HospitalId,
                        HospitalName = c.Hospital.Name,
                        CityName = c.Hospital.City.Name,
                        DepartmentName = c.Hospital.City.Department.Name
                    })
                    .Select(g => new TopHospitalModel
                    {
                        IdHospital = g.Key.HospitalId,
                        NombreHospital = g.Key.HospitalName ?? "Desconocido",
                        NombreMunicipio = g.Key.CityName ?? "",
                        NombreDepartamento = g.Key.DepartmentName ?? "",
                        TotalCasos = g.Count(),
                        CasosActivos = g.Count(c => c.StateId != 3 && c.StateId != 4)
                    })
                    .OrderByDescending(h => h.TotalCasos)
                    .Take(topLimit)
                    .ToList();

                return Ok(topHospitals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener top de hospitales", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene casos con coordenadas para visualización en mapa con filtro por año
        /// </summary>
        [HttpGet]
        [Route("mapCases")]
        [ProducesResponseType(typeof(List<MapCaseModel>), 200)]
        public async Task<IActionResult> GetMapCases([FromQuery] int? year)
        {
            try
            {
                var cases = await _caseRepository.GetAllAsync();
                var activeCases = cases.Where(c => c.IsActive);

                // Aplicar filtro por año si se proporciona
                if (year.HasValue)
                {
                    activeCases = activeCases.Where(c => c.Year == year.Value);
                }

                // Filtrar casos con coordenadas válidas
                var mapCases = activeCases
                    .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
                    .Select(c => new MapCaseModel
                    {
                        IdCaso = c.Id,
                        Latitud = c.Latitude!.Value,
                        Longitud = c.Longitude!.Value,
                        Descripcion = c.Description,
                        Direccion = c.Address,
                        Barrio = c.Neighborhood,
                        NombrePaciente = c.TemporaryName ?? c.Patient?.Name ?? "Anónimo",
                        NombreHospital = c.Hospital?.Name ?? "No asignado",
                        TipoDengue = c.TypeOfDengue?.Name ?? "Sin especificar",
                        IdTipoDengue = c.TypeOfDengueId,
                        Estado = c.State?.Name ?? "Desconocido",
                        IdEstado = c.StateId,
                        FechaRegistro = c.CreatedAt,
                        AnioReporte = c.Year,
                        EdadPaciente = c.Age
                    })
                    .ToList();

                return Ok(mapCases);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener casos para el mapa", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los años disponibles para filtrar en el mapa
        /// </summary>
        [HttpGet]
        [Route("availableYears")]
        [ProducesResponseType(typeof(List<int>), 200)]
        public async Task<IActionResult> GetAvailableYears()
        {
            try
            {
                var cases = await _caseRepository.GetAllAsync();
                var years = cases
                    .Where(c => c.IsActive && c.Year.HasValue)
                    .Select(c => c.Year!.Value)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList();

                return Ok(years);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener años disponibles", error = ex.Message });
            }
        }
    }
}
