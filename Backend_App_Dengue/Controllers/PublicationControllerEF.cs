using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Backend_App_Dengue.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("Publication")]
    [ApiController]
    public class PublicationControllerEF : ControllerBase
    {
        private readonly IRepository<Publication> _publicationRepository;
        private readonly IRepository<FCMToken> _fcmTokenRepository;
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<User> _userRepository;
        private readonly ConexionMongo _conexionMongo;
        private readonly FCMService _fcmService;

        public PublicationControllerEF(
            IRepository<Publication> publicationRepository,
            IRepository<FCMToken> fcmTokenRepository,
            IRepository<Notification> notificationRepository,
            IRepository<User> userRepository)
        {
            _publicationRepository = publicationRepository;
            _fcmTokenRepository = fcmTokenRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _conexionMongo = new ConexionMongo();
            _fcmService = new FCMService();
        }

        /// <summary>
        /// Get all publications with user information
        /// </summary>
        [HttpGet]
        [Route("getPublications")]
        public async Task<IActionResult> GetPublications()
        {
            try
            {
                var publications = await _publicationRepository.GetAllWithIncludesAsync(
                    p => p.User,
                    p => p.User.Role
                );

                var response = publications
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new PublicationResponseDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        UserId = p.UserId,
                        ImageId = p.ImageId,
                        IsActive = p.IsActive,
                        User = new UserInfoDto
                        {
                            Id = p.User.Id,
                            Name = p.User.Name,
                            Email = p.User.Email,
                            RoleName = p.User.Role?.Name
                        }
                    })
                    .ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las publicaciones", error = ex.Message });
            }
        }

        /// <summary>
        /// Search publications by title
        /// </summary>
        [HttpGet]
        [Route("getPublication")]
        [ProducesResponseType(typeof(List<Publication>), 200)]
        public async Task<IActionResult> GetPublication([FromQuery] string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                return BadRequest(new { message = "El nombre no puede estar vacío" });
            }

            try
            {
                var publications = await _publicationRepository.FindAsync(p =>
                    p.Title.Contains(nombre) && p.IsActive
                );

                return Ok(publications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar la publicación", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-007: Create a new publication with image upload to MongoDB and FCM notification
        /// </summary>
        [HttpPost]
        [Route("createPublication")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreatePublication([FromForm] CreatePublicationModelDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Faltan datos en la solicitud" });
            }

            if (dto.imagen == null)
            {
                return BadRequest(new { message = "La imagen es requerida" });
            }

            string? imagenId = null;

            try
            {
                // Upload image to MongoDB GridFS
                using (var stream = new MemoryStream())
                {
                    await dto.imagen.CopyToAsync(stream);
                    var imagenModel = new ImagenModel
                    {
                        Imagen = Convert.ToBase64String(stream.ToArray())
                    };
                    imagenId = _conexionMongo.UploadImage(imagenModel);
                }

                // Create publication in MySQL with EF Core
                var publication = new Publication
                {
                    Title = dto.Titulo,
                    Description = dto.Descripcion,
                    ImageId = imagenId,
                    UserId = int.Parse(dto.UsuarioId),
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var createdPublication = await _publicationRepository.AddAsync(publication);

                // Create individual notifications in database for all active users
                try
                {
                    var activeUsers = await _userRepository.FindAsync(u => u.IsActive);
                    var notificationContent = $"Se ha publicado: {dto.Titulo}. ¡Deberías verla!";

                    foreach (var user in activeUsers)
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

                // Send FCM push notification to all users
                try
                {
                    var allTokens = await _fcmTokenRepository.FindAsync(t => t.User.IsActive);
                    var tokens = allTokens.Select(t => t.Token).ToList();

                    if (tokens.Count > 0)
                    {
                        var data = new Dictionary<string, string>
                        {
                            { "type", "new_publication" },
                            { "titulo", dto.Titulo }
                        };

                        await _fcmService.SendNotificationToMultipleDevices(
                            tokens,
                            "Nueva Publicación",
                            $"Se ha publicado: {dto.Titulo}. ¡Deberías verla!",
                            data
                        );
                    }
                }
                catch (Exception fcmEx)
                {
                    Console.WriteLine($"Error al enviar notificación push: {fcmEx.Message}");
                    // Don't fail publication creation if notification fails
                }

                return Ok(new { message = "Publicación creada con éxito", imagenId = imagenId });
            }
            catch (Exception ex)
            {
                // If failed after uploading image, try to delete it
                if (!string.IsNullOrEmpty(imagenId))
                {
                    try { _conexionMongo.DeleteImage(imagenId); } catch { }
                }

                return StatusCode(500, new { message = "Error al insertar la publicación", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-007: Get publication by ID with user information
        /// </summary>
        [HttpGet]
        [Route("getPublicationById/{id}")]
        public async Task<IActionResult> GetPublicationById(int id)
        {
            try
            {
                var publications = await _publicationRepository.GetAllWithIncludesAsync(
                    p => p.User,
                    p => p.User.Role
                );

                var publication = publications.FirstOrDefault(p => p.Id == id);

                if (publication == null)
                {
                    return NotFound(new { message = "Publicación no encontrada" });
                }

                var response = new PublicationResponseDto
                {
                    Id = publication.Id,
                    Title = publication.Title,
                    Description = publication.Description,
                    CreatedAt = publication.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UserId = publication.UserId,
                    ImageId = publication.ImageId,
                    IsActive = publication.IsActive,
                    User = new UserInfoDto
                    {
                        Id = publication.User.Id,
                        Name = publication.User.Name,
                        Email = publication.User.Email,
                        RoleName = publication.User.Role?.Name
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la publicación", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-007: Update publication
        /// </summary>
        [HttpPut]
        [Route("updatePublication/{id}")]
        public async Task<IActionResult> UpdatePublication(int id, [FromBody] UpdatePublicationDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos de la publicación son requeridos" });
            }

            try
            {
                var publication = await _publicationRepository.GetByIdAsync(id);

                if (publication == null)
                {
                    return NotFound(new { message = "Publicación no encontrada" });
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(dto.Titulo))
                {
                    publication.Title = dto.Titulo;
                }

                if (!string.IsNullOrWhiteSpace(dto.Descripcion))
                {
                    publication.Description = dto.Descripcion;
                }

                await _publicationRepository.UpdateAsync(publication);

                return Ok(new { message = "Publicación actualizada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la publicación", error = ex.Message });
            }
        }

        /// <summary>
        /// HU-007: Delete publication (soft delete)
        /// </summary>
        [HttpDelete]
        [Route("deletePublication/{id}")]
        public async Task<IActionResult> DeletePublication(int id)
        {
            try
            {
                var publication = await _publicationRepository.GetByIdAsync(id);

                if (publication == null)
                {
                    return NotFound(new { message = "Publicación no encontrada" });
                }

                // Soft delete
                publication.IsActive = false;
                await _publicationRepository.UpdateAsync(publication);

                return Ok(new { message = "Publicación eliminada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la publicación", error = ex.Message });
            }
        }
    }
}
