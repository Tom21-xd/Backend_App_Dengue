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
        private readonly IRepository<PublicationReaction> _reactionRepository;
        private readonly IRepository<PublicationComment> _commentRepository;
        private readonly IRepository<SavedPublication> _savedPublicationRepository;
        private readonly ConexionMongo _conexionMongo;
        private readonly FCMService _fcmService;

        public PublicationControllerEF(
            IRepository<Publication> publicationRepository,
            IRepository<FCMToken> fcmTokenRepository,
            IRepository<Notification> notificationRepository,
            IRepository<User> userRepository,
            IRepository<PublicationReaction> reactionRepository,
            IRepository<PublicationComment> commentRepository,
            IRepository<SavedPublication> savedPublicationRepository,
            FCMService fcmService)
        {
            _publicationRepository = publicationRepository;
            _fcmTokenRepository = fcmTokenRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _reactionRepository = reactionRepository;
            _commentRepository = commentRepository;
            _savedPublicationRepository = savedPublicationRepository;
            _conexionMongo = new ConexionMongo();
            _fcmService = fcmService;
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

        // ==================== REACCIONES ====================

        /// <summary>
        /// Toggle reaction on a publication (add or remove)
        /// </summary>
        [HttpPost]
        [Route("toggleReaction/{publicationId}/{userId}")]
        public async Task<IActionResult> ToggleReaction(int publicationId, int userId)
        {
            try
            {
                var type = "MeGusta"; // Default reaction type

                // Check if reaction already exists
                var existingReaction = await _reactionRepository.FindAsync(r =>
                    r.PublicationId == publicationId && r.UserId == userId);

                var reaction = existingReaction.FirstOrDefault();

                if (reaction != null)
                {
                    // Remove reaction
                    await _reactionRepository.DeleteAsync(reaction);
                    return Ok(new { message = "Reacción eliminada", hasReaction = false });
                }
                else
                {
                    // Add reaction
                    var newReaction = new PublicationReaction
                    {
                        PublicationId = publicationId,
                        UserId = userId,
                        ReactionType = type,
                        CreatedAt = DateTime.Now
                    };
                    await _reactionRepository.AddAsync(newReaction);
                    return Ok(new { message = "Reacción agregada", hasReaction = true });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al procesar la reacción", error = ex.Message });
            }
        }

        /// <summary>
        /// Get reactions count for a publication
        /// </summary>
        [HttpGet]
        [Route("getReactions/{publicationId}")]
        public async Task<IActionResult> GetReactions(int publicationId)
        {
            try
            {
                var reactions = await _reactionRepository.FindAsync(r => r.PublicationId == publicationId);
                var total = reactions.Count();

                return Ok(new { total = total, reactions = reactions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener reacciones", error = ex.Message });
            }
        }

        // ==================== COMENTARIOS ====================

        /// <summary>
        /// Get all comments for a publication
        /// </summary>
        [HttpGet]
        [Route("getComments/{publicationId}")]
        public async Task<IActionResult> GetComments(int publicationId)
        {
            try
            {
                // Cargar comentarios con usuario y respuestas anidadas
                var allComments = await _commentRepository.GetAllWithIncludesAsync(
                    c => c.User,
                    c => c.Replies
                );

                var publicationComments = allComments
                    .Where(c => c.PublicationId == publicationId && c.IsActive)
                    .OrderBy(c => c.CreatedAt)
                    .ToList();

                // Las respuestas también deben cargar el usuario automáticamente gracias a GetAllWithIncludesAsync

                return Ok(publicationComments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener comentarios", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new comment on a publication
        /// </summary>
        [HttpPost]
        [Route("createComment/{publicationId}/{userId}")]
        public async Task<IActionResult> CreateComment(int publicationId, int userId, [FromBody] CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest(new { message = "El comentario no puede estar vacío" });
            }

            try
            {
                var comment = new PublicationComment
                {
                    PublicationId = publicationId,
                    UserId = userId,
                    Content = dto.Content,
                    ParentCommentId = dto.ParentCommentId,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var createdComment = await _commentRepository.AddAsync(comment);

                // Load user info for response
                var commentWithUser = await _commentRepository.GetAllWithIncludesAsync(c => c.User);
                var result = commentWithUser.FirstOrDefault(c => c.Id == createdComment.Id);

                return Ok(new { message = "Comentario creado", comment = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear comentario", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a comment (soft delete)
        /// </summary>
        [HttpDelete]
        [Route("deleteComment/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentId);

                if (comment == null)
                {
                    return NotFound(new { message = "Comentario no encontrado" });
                }

                comment.IsActive = false;
                await _commentRepository.UpdateAsync(comment);

                return Ok(new { message = "Comentario eliminado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar comentario", error = ex.Message });
            }
        }

        // ==================== GUARDADOS ====================

        /// <summary>
        /// Toggle saved publication (save or unsave)
        /// </summary>
        [HttpPost]
        [Route("toggleSave/{publicationId}/{userId}")]
        public async Task<IActionResult> ToggleSave(int publicationId, int userId)
        {
            try
            {
                var existingSave = await _savedPublicationRepository.FindAsync(s =>
                    s.PublicationId == publicationId && s.UserId == userId);

                var saved = existingSave.FirstOrDefault();

                if (saved != null)
                {
                    // Remove save
                    await _savedPublicationRepository.DeleteAsync(saved);
                    return Ok(new { message = "Publicación removida de guardados", isSaved = false });
                }
                else
                {
                    // Save publication
                    var newSave = new SavedPublication
                    {
                        PublicationId = publicationId,
                        UserId = userId,
                        SavedAt = DateTime.Now
                    };
                    await _savedPublicationRepository.AddAsync(newSave);
                    return Ok(new { message = "Publicación guardada", isSaved = true });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al guardar publicación", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all saved publications for a user
        /// </summary>
        [HttpGet]
        [Route("getSavedPublications/{userId}")]
        public async Task<IActionResult> GetSavedPublications(int userId)
        {
            try
            {
                // Load saved publications with full publication data including nested entities
                var savedPubs = await _savedPublicationRepository.GetAllWithIncludesAsync(
                    s => s.Publication,
                    s => s.Publication.User,
                    s => s.Publication.User.Role,
                    s => s.Publication.Category,
                    s => s.Publication.City,
                    s => s.Publication.Department,
                    s => s.Publication.Reactions,
                    s => s.Publication.Comments,
                    s => s.Publication.SavedByUsers
                );

                var userSaved = savedPubs
                    .Where(s => s.UserId == userId && s.Publication != null)
                    .OrderByDescending(s => s.SavedAt)
                    .Select(s => new
                    {
                        ID_GUARDADO = s.Id,
                        FK_ID_PUBLICACION = s.PublicationId,
                        FK_ID_USUARIO = s.UserId,
                        FECHA_GUARDADO = s.SavedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        PUBLICACION = BuildPublicationResponse(s.Publication, userId)
                    })
                    .ToList();

                return Ok(userSaved);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener publicaciones guardadas", error = ex.Message });
            }
        }

        private object BuildPublicationResponse(Publication pub, int currentUserId)
        {
            return new
            {
                ID_PUBLICACION = pub.Id,
                TITULO_PUBLICACION = pub.Title,
                DESCRIPCION_PUBLICACION = pub.Description,
                FECHA_PUBLICACION = pub.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                FK_ID_USUARIO = pub.UserId,
                IMAGEN_PUBLICACION = pub.ImageId,
                ESTADO_PUBLICACION = pub.IsActive,
                FK_ID_CATEGORIA = pub.CategoryId,
                NIVEL_PRIORIDAD = pub.Priority,
                FIJADA = pub.IsPinned,
                FECHA_EXPIRACION = pub.ExpirationDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                LATITUD = pub.Latitude,
                LONGITUD = pub.Longitude,
                FK_ID_CIUDAD = pub.CityId,
                FK_ID_DEPARTAMENTO = pub.DepartmentId,
                DIRECCION = pub.Address,
                ENVIAR_NOTIFICACION_PUSH = pub.SendPushNotification,
                NOTIFICACION_ENVIADA = pub.NotificationSent,
                FECHA_ENVIO_NOTIFICACION = pub.NotificationSentAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                FECHA_PUBLICACION_PROGRAMADA = pub.ScheduledPublishDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                PUBLICADA = pub.IsPublished,
                USUARIO = pub.User != null ? new
                {
                    ID_USUARIO = pub.User.Id,
                    NOMBRE_USUARIO = pub.User.Name,
                    CORREO_USUARIO = pub.User.Email,
                    FK_ID_ROL = pub.User.RoleId,
                    ROL = pub.User.Role != null ? new
                    {
                        ID_ROL = pub.User.Role.Id,
                        NOMBRE_ROL = pub.User.Role.Name
                    } : null
                } : null,
                CATEGORIA = pub.Category != null ? new
                {
                    ID_CATEGORIA = pub.Category.Id,
                    NOMBRE_CATEGORIA = pub.Category.Name,
                    DESCRIPCION_CATEGORIA = pub.Category.Description,
                    ICONO = pub.Category.Icon,
                    COLOR = pub.Category.Color
                } : null,
                CIUDAD = pub.City != null ? new
                {
                    ID_CIUDAD = pub.City.Id,
                    NOMBRE_CIUDAD = pub.City.Name
                } : null,
                DEPARTAMENTO = pub.Department != null ? new
                {
                    ID_DEPARTAMENTO = pub.Department.Id,
                    NOMBRE_DEPARTAMENTO = pub.Department.Name
                } : null,
                TOTAL_REACCIONES = pub.Reactions?.Count ?? 0,
                TOTAL_COMENTARIOS = pub.Comments?.Count ?? 0,
                TOTAL_GUARDADOS = pub.SavedByUsers?.Count ?? 0,
                USUARIO_HA_REACCIONADO = pub.Reactions?.Any(r => r.UserId == currentUserId) ?? false,
                USUARIO_HA_GUARDADO = pub.SavedByUsers?.Any(s => s.UserId == currentUserId) ?? false
            };
        }

        /// <summary>
        /// Get publication statistics (reactions, comments, saves count)
        /// </summary>
        [HttpGet]
        [Route("getStats/{publicationId}")]
        public async Task<IActionResult> GetPublicationStats(int publicationId)
        {
            try
            {
                var reactions = await _reactionRepository.FindAsync(r => r.PublicationId == publicationId);
                var comments = await _commentRepository.FindAsync(c => c.PublicationId == publicationId && c.IsActive);
                var saves = await _savedPublicationRepository.FindAsync(s => s.PublicationId == publicationId);

                var stats = new
                {
                    totalReactions = reactions.Count(),
                    totalComments = comments.Count(),
                    totalSaves = saves.Count()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if user has interacted with publication
        /// </summary>
        [HttpGet]
        [Route("getUserInteractions/{publicationId}/{userId}")]
        public async Task<IActionResult> GetUserInteractions(int publicationId, int userId)
        {
            try
            {
                var hasReaction = await _reactionRepository.FindAsync(r =>
                    r.PublicationId == publicationId && r.UserId == userId);
                var hasSaved = await _savedPublicationRepository.FindAsync(s =>
                    s.PublicationId == publicationId && s.UserId == userId);

                var interactions = new
                {
                    hasReacted = hasReaction.Any(),
                    hasSaved = hasSaved.Any()
                };

                return Ok(interactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al verificar interacciones", error = ex.Message });
            }
        }
    }
}
