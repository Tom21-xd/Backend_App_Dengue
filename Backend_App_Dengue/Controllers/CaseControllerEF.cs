using Backend_App_Dengue.Attributes;
using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Enums;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model.Dto;
using Backend_App_Dengue.Services;
using Backend_App_Dengue.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend_App_Dengue.Controllers
{
    /// <summary>
    /// Controlador para gestión de casos de dengue con notificaciones en tiempo real
    /// </summary>
    [Route("Case")]
    [ApiController]
    [Produces("application/json")]
    public class CaseControllerEF : ControllerBase
    {
        private readonly IRepository<Case> _caseRepository;
        private readonly IRepository<FCMToken> _fcmTokenRepository;
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<User> _userRepository;
        private readonly FCMService _fcmService;
        private readonly AppDbContext _context;
        private readonly IHubContext<CaseHub> _hubContext;

        public CaseControllerEF(
            IRepository<Case> caseRepository,
            IRepository<FCMToken> fcmTokenRepository,
            IRepository<Notification> notificationRepository,
            IRepository<User> userRepository,
            FCMService fcmService,
            AppDbContext context,
            IHubContext<CaseHub> hubContext)
        {
            _caseRepository = caseRepository;
            _fcmTokenRepository = fcmTokenRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _fcmService = fcmService;
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Obtiene todos los casos de dengue con entidades relacionadas
        /// </summary>
        /// <returns>Lista completa de casos activos con datos de paciente, hospital, tipo de dengue y estado</returns>
        /// <response code="200">Lista de casos obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [Route("getCases")]
        [RequirePermission(PermissionCode.CASE_VIEW_ALL)]
        [ProducesResponseType(typeof(List<Case>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCases()
        {
            try
            {
                var cases = await _context.Cases
                    .Include(c => c.State)
                    .Include(c => c.Hospital)
                        .ThenInclude(h => h.City)
                            .ThenInclude(ci => ci.Department)
                    .Include(c => c.TypeOfDengue)
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.Role)
                    .Include(c => c.MedicalStaff)
                        .ThenInclude(m => m!.Role)
                    .Where(c => c.IsActive)
                    .ToListAsync();

                var response = cases.Select(c => new CaseResponseDto
                {
                    Id = c.Id,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    StateId = c.StateId,
                    HospitalId = c.HospitalId,
                    TypeOfDengueId = c.TypeOfDengueId,
                    PatientId = c.PatientId,
                    MedicalStaffId = c.MedicalStaffId,
                    FinishedAt = c.FinishedAt,
                    Address = c.Address,
                    IsActive = c.IsActive,
                    Year = c.Year,
                    Age = c.Age,
                    TemporaryName = c.TemporaryName,
                    Neighborhood = c.Neighborhood,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    RegisteredByUserId = c.RegisteredByUserId,
                    State = c.State != null ? new CaseStateInfoDto
                    {
                        Id = c.State.Id,
                        Name = c.State.Name
                    } : null,
                    Hospital = c.Hospital != null ? new HospitalInfoDto
                    {
                        Id = c.Hospital.Id,
                        Name = c.Hospital.Name,
                        Address = c.Hospital.Address,
                        Latitude = c.Hospital.Latitude,
                        Longitude = c.Hospital.Longitude,
                        CityId = c.Hospital.CityId,
                        City = c.Hospital.City != null ? new CityInfoDto
                        {
                            Id = c.Hospital.City.Id,
                            Name = c.Hospital.City.Name,
                            DepartmentId = c.Hospital.City.DepartmentId,
                            Department = c.Hospital.City.Department != null ? new DepartmentInfoDto
                            {
                                Id = c.Hospital.City.Department.Id,
                                Name = c.Hospital.City.Department.Name
                            } : null
                        } : null
                    } : null,
                    TypeOfDengue = c.TypeOfDengue != null ? new TypeOfDengueInfoDto
                    {
                        Id = c.TypeOfDengue.Id,
                        Name = c.TypeOfDengue.Name
                    } : null,
                    Patient = c.Patient != null ? new UserInfoDto
                    {
                        Id = c.Patient.Id,
                        Name = c.Patient.Name,
                        Email = c.Patient.Email,
                        RoleName = c.Patient.Role?.Name,
                        BirthDate = c.Patient.BirthDate
                    } : null,
                    MedicalStaff = c.MedicalStaff != null ? new UserInfoDto
                    {
                        Id = c.MedicalStaff.Id,
                        Name = c.MedicalStaff.Name,
                        Email = c.MedicalStaff.Email,
                        RoleName = c.MedicalStaff.Role?.Name,
                        BirthDate = c.MedicalStaff.BirthDate
                    } : null
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los casos", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo caso de dengue con notificación FCM al personal médico
        /// Solo personal médico y administradores
        /// </summary>
        [HttpPost]
        [Route("createCase")]
        [RequirePermission(PermissionCode.CASE_CREATE)]
        public async Task<IActionResult> CreateCase([FromBody] CreateCaseModelDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos del caso son requeridos" });
            }

            try
            {
                int? NormalizeId(int? idValue) => idValue.HasValue && idValue.Value > 0 ? idValue : null;

                var hospitalId = NormalizeId(dto.id_hospital);
                var patientId = NormalizeId(dto.id_paciente);
                var medicalStaffId = NormalizeId(dto.id_personalMedico);

                if (dto.id_tipoDengue <= 0)
                {
                    return BadRequest(new { message = "El tipo de dengue seleccionado no es válido" });
                }

                if (patientId == null)
                {
                    var hasTemporaryInfo = !string.IsNullOrWhiteSpace(dto.nombre_temporal) || dto.edad.HasValue;
                    if (!hasTemporaryInfo)
                    {
                        return BadRequest(new
                        {
                            message = "Para crear un caso sin usuario registrado debes proporcionar un nombre temporal o la edad del paciente"
                        });
                    }
                }

                string? address = !string.IsNullOrWhiteSpace(dto.direccion)
                    ? dto.direccion.Trim()
                    : string.IsNullOrWhiteSpace(dto.barrio) ? null : dto.barrio.Trim();

                string? tempName = string.IsNullOrWhiteSpace(dto.nombre_temporal) ? null : dto.nombre_temporal.Trim();
                string? neighborhood = string.IsNullOrWhiteSpace(dto.barrio) ? null : dto.barrio.Trim();

                int? registeredByUserId = null;
                var userIdClaim = User?.FindFirst("userId")
                    ?? User?.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User?.FindFirst("sub")
                    ?? User?.FindFirst("id");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsedUserId))
                {
                    registeredByUserId = parsedUserId;
                }

                // Crear nuevo caso
                var newCase = new Case
                {
                    Description = dto.descripcion,
                    HospitalId = hospitalId,
                    TypeOfDengueId = dto.id_tipoDengue,
                    PatientId = patientId,
                    MedicalStaffId = medicalStaffId,
                    Address = address,
                    StateId = 1, // Estado por defecto (Reportado/En proceso)
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    Year = dto.anio_reporte,
                    Age = dto.edad,
                    TemporaryName = tempName,
                    Neighborhood = neighborhood,
                    Latitude = dto.latitud,
                    Longitude = dto.longitud,
                    RegisteredByUserId = registeredByUserId
                };

                var createdCase = await _caseRepository.AddAsync(newCase);

                // Enviar notificación en tiempo real vía SignalR a todos los clientes conectados
                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNewCase", createdCase.Id, "Nuevo caso de dengue reportado");
                    Console.WriteLine($"SignalR notification sent for new case: {createdCase.Id}");
                }
                catch (Exception signalREx)
                {
                    Console.WriteLine($"Error al enviar notificación SignalR: {signalREx.Message}");
                }

                // Crear notificaciones individuales en base de datos solo para personal médico (rol 3)
                try
                {
                    var medicalStaff = await _userRepository.FindAsync(u => u.RoleId == 3 && u.IsActive);
                    var notificationContent = $"Se ha reportado un nuevo caso de dengue. ¡Deberías revisarlo!";

                    foreach (var user in medicalStaff)
                    {
                        var notification = new Notification
                        {
                            Content = notificationContent,
                            UserId = user.Id,
                            CreatedAt = DateTime.Now,
                            IsRead = false,
                            IsActive = true
                        };
                        await _notificationRepository.AddAsync(notification);
                    }
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"Error al crear notificaciones en BD: {notifEx.Message}");
                }

                // Enviar notificación push FCM al personal médico (rol 3)
                try
                {
                    // Obtener todos los tokens FCM del personal médico (rol 3)
                    var medicalStaffTokens = await _fcmTokenRepository.FindAsync(t =>
                        t.User.RoleId == 3 && t.User.IsActive
                    );

                    var tokens = medicalStaffTokens.Select(t => t.Token).ToList();

                    if (tokens.Count > 0)
                    {
                        var data = new Dictionary<string, string>
                        {
                            { "type", "new_case" },
                            { "caso_id", createdCase.Id.ToString() }
                        };

                        await _fcmService.SendNotificationToMultipleDevices(
                            tokens,
                            "Nuevo Caso de Dengue",
                            $"Se ha reportado un nuevo caso de dengue. ¡Deberías revisarlo!",
                            data
                        );
                    }
                }
                catch (Exception fcmEx)
                {
                    Console.WriteLine($"Error al enviar notificación push: {fcmEx.Message}");
                    // No fallar la creación del caso si falla la notificación
                }

                return Ok(new { message = "Se ha creado el caso con éxito", case_id = createdCase.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el caso", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un caso por ID con entidades anidadas
        /// </summary>
        [HttpGet]
        [Route("getCaseById")]
        [RequirePermission(PermissionCode.CASE_VIEW)]
        public async Task<IActionResult> GetCaseById([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "El ID del caso es requerido" });
            }

            try
            {
                if (!int.TryParse(id, out int caseId))
                {
                    return BadRequest(new { message = "El ID del caso debe ser un número válido" });
                }

                var c = await _context.Cases
                    .Include(c => c.State)
                    .Include(c => c.Hospital)
                        .ThenInclude(h => h.City)
                            .ThenInclude(ci => ci.Department)
                    .Include(c => c.TypeOfDengue)
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.Role)
                    .Include(c => c.MedicalStaff)
                        .ThenInclude(m => m!.Role)
                    .FirstOrDefaultAsync(c => c.Id == caseId);

                if (c == null)
                {
                    return NotFound(new { message = "No se ha encontrado el caso" });
                }

                var response = new CaseResponseDto
                {
                    Id = c.Id,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    StateId = c.StateId,
                    HospitalId = c.HospitalId,
                    TypeOfDengueId = c.TypeOfDengueId,
                    PatientId = c.PatientId,
                    MedicalStaffId = c.MedicalStaffId,
                    FinishedAt = c.FinishedAt,
                    Address = c.Address,
                    IsActive = c.IsActive,
                    State = c.State != null ? new CaseStateInfoDto
                    {
                        Id = c.State.Id,
                        Name = c.State.Name
                    } : null,
                    Hospital = c.Hospital != null ? new HospitalInfoDto
                    {
                        Id = c.Hospital.Id,
                        Name = c.Hospital.Name,
                        Address = c.Hospital.Address,
                        Latitude = c.Hospital.Latitude,
                        Longitude = c.Hospital.Longitude,
                        CityId = c.Hospital.CityId,
                        City = c.Hospital.City != null ? new CityInfoDto
                        {
                            Id = c.Hospital.City.Id,
                            Name = c.Hospital.City.Name,
                            DepartmentId = c.Hospital.City.DepartmentId,
                            Department = c.Hospital.City.Department != null ? new DepartmentInfoDto
                            {
                                Id = c.Hospital.City.Department.Id,
                                Name = c.Hospital.City.Department.Name
                            } : null
                        } : null
                    } : null,
                    TypeOfDengue = c.TypeOfDengue != null ? new TypeOfDengueInfoDto
                    {
                        Id = c.TypeOfDengue.Id,
                        Name = c.TypeOfDengue.Name
                    } : null,
                    Patient = c.Patient != null ? new UserInfoDto
                    {
                        Id = c.Patient.Id,
                        Name = c.Patient.Name,
                        Email = c.Patient.Email,
                        RoleName = c.Patient.Role?.Name,
                        BirthDate = c.Patient.BirthDate
                    } : null,
                    MedicalStaff = c.MedicalStaff != null ? new UserInfoDto
                    {
                        Id = c.MedicalStaff.Id,
                        Name = c.MedicalStaff.Name,
                        Email = c.MedicalStaff.Email,
                        RoleName = c.MedicalStaff.Role?.Name,
                        BirthDate = c.MedicalStaff.BirthDate
                    } : null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el caso", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un caso con notificación FCM
        /// Solo personal médico y administradores
        /// </summary>
        [HttpPatch]
        [Route("updateCase/{id}")]
        [RequirePermission(PermissionCode.CASE_UPDATE)]
        public async Task<IActionResult> UpdateCase(int id, [FromBody] UpdateCaseDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos de actualización son requeridos" });
            }

            try
            {
                var existingCase = await _caseRepository.GetByIdAsync(id);

                if (existingCase == null)
                {
                    return NotFound(new { message = "Caso no encontrado" });
                }

                // Actualizar solo los campos proporcionados
                if (dto.IdEstadoCaso.HasValue)
                {
                    existingCase.StateId = dto.IdEstadoCaso.Value;
                }

                if (dto.IdTipoDengue.HasValue)
                {
                    existingCase.TypeOfDengueId = dto.IdTipoDengue.Value;
                }

                if (!string.IsNullOrWhiteSpace(dto.Descripcion))
                {
                    existingCase.Description = dto.Descripcion;
                }

                await _caseRepository.UpdateAsync(existingCase);

                // Enviar notificación en tiempo real vía SignalR
                try
                {
                    bool casoFinalizado = dto.IdEstadoCaso.HasValue && dto.IdEstadoCaso.Value == 3;
                    string mensaje = casoFinalizado
                        ? $"Caso #{id} finalizado"
                        : $"Caso #{id} actualizado";

                    await _hubContext.Clients.All.SendAsync("ReceiveCaseUpdate", id, mensaje);
                    Console.WriteLine($"SignalR notification sent for case update: {id}");
                }
                catch (Exception signalREx)
                {
                    Console.WriteLine($"Error al enviar notificación SignalR: {signalREx.Message}");
                }

                // Enviar notificaciones push FCM
                try
                {
                    // Obtener todos los tokens FCM del personal médico (rol 3)
                    var medicalStaffTokens = await _fcmTokenRepository.FindAsync(t =>
                        t.User.RoleId == 3 && t.User.IsActive
                    );

                    var tokens = medicalStaffTokens.Select(t => t.Token).ToList();

                    if (tokens.Count > 0)
                    {
                        // Verificar si el caso fue finalizado (estado 3 = Finalizado)
                        bool casoFinalizado = dto.IdEstadoCaso.HasValue && dto.IdEstadoCaso.Value == 3;

                        var data = new Dictionary<string, string>
                        {
                            { "type", casoFinalizado ? "case_finished" : "case_updated" },
                            { "caso_id", id.ToString() }
                        };

                        string titulo = casoFinalizado ? "Caso Finalizado" : "Caso Actualizado";
                        string mensaje = casoFinalizado
                            ? $"El caso #{id} ha sido finalizado."
                            : $"El caso #{id} ha sido actualizado. Revisa los cambios.";

                        await _fcmService.SendNotificationToMultipleDevices(
                            tokens,
                            titulo,
                            mensaje,
                            data
                        );
                    }
                }
                catch (Exception fcmEx)
                {
                    Console.WriteLine($"Error al enviar notificación push: {fcmEx.Message}");
                    // No fallar la actualización si falla la notificación
                }

                return Ok(new { message = "Caso actualizado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el caso", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-006: Elimina un caso (eliminación lógica)
        /// Solo administradores
        /// </summary>
        [HttpDelete]
        [Route("deleteCase/{id}")]
        [RequirePermission(PermissionCode.CASE_DELETE)]
        public async Task<IActionResult> DeleteCase(int id)
        {
            try
            {
                var caso = await _caseRepository.GetByIdAsync(id);

                if (caso == null)
                {
                    return NotFound(new { message = "Caso no encontrado" });
                }

                // Eliminación lógica - marcar como inactivo
                caso.IsActive = false;
                await _caseRepository.UpdateAsync(caso);

                // Enviar notificación en tiempo real vía SignalR
                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveCaseDeleted", id);
                    Console.WriteLine($"SignalR notification sent for case deletion: {id}");
                }
                catch (Exception signalREx)
                {
                    Console.WriteLine($"Error al enviar notificación SignalR: {signalREx.Message}");
                }

                return Ok(new { message = "Caso eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el caso", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza las coordenadas de un caso específico
        /// Usado después de importación masiva para ajustar ubicaciones
        /// </summary>
        [HttpPatch]
        [Route("updateCoordinates/{id}")]
        [RequirePermission(PermissionCode.CASE_UPDATE)]
        public async Task<IActionResult> UpdateCaseCoordinates(int id, [FromBody] UpdateCaseCoordinatesDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Las coordenadas son requeridas" });
            }

            try
            {
                var existingCase = await _caseRepository.GetByIdAsync(id);

                if (existingCase == null)
                {
                    return NotFound(new { message = "Caso no encontrado" });
                }

                existingCase.Latitude = dto.Latitude;
                existingCase.Longitude = dto.Longitude;

                await _caseRepository.UpdateAsync(existingCase);

                return Ok(new {
                    message = "Coordenadas actualizadas con éxito",
                    caseId = id,
                    latitude = dto.Latitude,
                    longitude = dto.Longitude
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar las coordenadas", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-012: Obtiene el historial de casos de un paciente
        /// </summary>
        [HttpGet]
        [Route("getCaseHistory/{userId}")]
        public async Task<IActionResult> GetCaseHistory(int userId)
        {
            try
            {
                var cases = await _caseRepository.FindAsync(c => c.PatientId == userId);
                return Ok(cases);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el historial de casos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene casos por hospital
        /// </summary>
        [HttpGet]
        [Route("getCasesByHospital/{hospitalId}")]
        public async Task<IActionResult> GetCasesByHospital(int hospitalId)
        {
            try
            {
                var cases = await _caseRepository.FindAsync(c => c.HospitalId == hospitalId && c.IsActive);
                return Ok(cases);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los casos del hospital", error = ex.Message });
            }
        }
    }
}
