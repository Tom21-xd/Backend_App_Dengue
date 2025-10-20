using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model.Dto;
using Backend_App_Dengue.Services;
using Backend_App_Dengue.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    [Route("Case")]
    [ApiController]
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
        /// Get all cases with nested entities
        /// </summary>
        [HttpGet]
        [Route("getCases")]
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
                        RoleName = c.Patient.Role?.Name
                    } : null,
                    MedicalStaff = c.MedicalStaff != null ? new UserInfoDto
                    {
                        Id = c.MedicalStaff.Id,
                        Name = c.MedicalStaff.Name,
                        Email = c.MedicalStaff.Email,
                        RoleName = c.MedicalStaff.Role?.Name
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
        /// Create a new dengue case with FCM notification to medical staff
        /// </summary>
        [HttpPost]
        [Route("createCase")]
        public async Task<IActionResult> CreateCase([FromBody] CreateCaseModelDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos del caso son requeridos" });
            }

            try
            {
                // Create new case
                var newCase = new Case
                {
                    Description = dto.descripcion,
                    HospitalId = dto.id_hospital,
                    TypeOfDengueId = dto.id_tipoDengue,
                    PatientId = dto.id_paciente,
                    MedicalStaffId = dto.id_personalMedico,
                    Address = dto.direccion,
                    StateId = 1, // Default state (Reportado/En proceso)
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var createdCase = await _caseRepository.AddAsync(newCase);

                // Send real-time notification via SignalR to all connected clients
                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNewCase", createdCase.Id, "Nuevo caso de dengue reportado");
                    Console.WriteLine($"SignalR notification sent for new case: {createdCase.Id}");
                }
                catch (Exception signalREx)
                {
                    Console.WriteLine($"Error al enviar notificación SignalR: {signalREx.Message}");
                }

                // Create individual notifications in database for medical staff only (role 3)
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

                // Send FCM push notification to medical staff (role 3)
                try
                {
                    // Get all FCM tokens for medical staff (role 3)
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
                    // Don't fail case creation if notification fails
                }

                return Ok(new { message = "Se ha creado el caso con éxito", case_id = createdCase.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el caso", error = ex.Message });
            }
        }

        /// <summary>
        /// Get case by ID with nested entities
        /// </summary>
        [HttpGet]
        [Route("getCaseById")]
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
                        RoleName = c.Patient.Role?.Name
                    } : null,
                    MedicalStaff = c.MedicalStaff != null ? new UserInfoDto
                    {
                        Id = c.MedicalStaff.Id,
                        Name = c.MedicalStaff.Name,
                        Email = c.MedicalStaff.Email,
                        RoleName = c.MedicalStaff.Role?.Name
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
        /// Update a case with FCM notification
        /// </summary>
        [HttpPatch]
        [Route("updateCase/{id}")]
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

                // Update only provided fields
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

                // Send real-time notification via SignalR
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

                // Send FCM push notifications
                try
                {
                    // Get all FCM tokens for medical staff (role 3)
                    var medicalStaffTokens = await _fcmTokenRepository.FindAsync(t =>
                        t.User.RoleId == 3 && t.User.IsActive
                    );

                    var tokens = medicalStaffTokens.Select(t => t.Token).ToList();

                    if (tokens.Count > 0)
                    {
                        // Check if case was finalized (state 3 = Finalizado)
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
                    // Don't fail update if notification fails
                }

                return Ok(new { message = "Caso actualizado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el caso", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-006: Delete a case (soft delete)
        /// </summary>
        [HttpDelete]
        [Route("deleteCase/{id}")]
        public async Task<IActionResult> DeleteCase(int id)
        {
            try
            {
                var caso = await _caseRepository.GetByIdAsync(id);

                if (caso == null)
                {
                    return NotFound(new { message = "Caso no encontrado" });
                }

                // Soft delete - mark as inactive
                caso.IsActive = false;
                await _caseRepository.UpdateAsync(caso);

                // Send real-time notification via SignalR
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
        /// HU-012: Get case history for a patient
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
        /// Get cases by hospital
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
