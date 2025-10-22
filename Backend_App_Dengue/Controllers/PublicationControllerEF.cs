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
        private readonly IRepository<PublicationView> _viewRepository;
        private readonly IRepository<PublicationTagRelation> _tagRelationRepository;
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
            IRepository<PublicationView> viewRepository,
            IRepository<PublicationTagRelation> tagRelationRepository,
            FCMService fcmService)
        {
            _publicationRepository = publicationRepository;
            _fcmTokenRepository = fcmTokenRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _reactionRepository = reactionRepository;
            _commentRepository = commentRepository;
            _savedPublicationRepository = savedPublicationRepository;
            _viewRepository = viewRepository;
            _tagRelationRepository = tagRelationRepository;
            _conexionMongo = new ConexionMongo();
            _fcmService = fcmService;
        }

        /// <summary>
        /// Get all publications with user information and interaction counters
        /// </summary>
        [HttpGet]
        [Route("getPublications")]
        public async Task<IActionResult> GetPublications([FromQuery] int? userId = null)
        {
            try
            {
                var publications = await _publicationRepository.GetAllWithIncludesAsync(
                    p => p.User,
                    p => p.User.Role
                );

                // Build response list with counters (using async methods in loop)
                var response = new List<PublicationResponseDto>();

                foreach (var p in publications.Where(p => p.IsActive).OrderByDescending(p => p.CreatedAt))
                {
                    response.Add(new PublicationResponseDto
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
                        },
                        // Calculate interaction counters
                        TotalReactions = await _reactionRepository.CountAsync(r => r.PublicationId == p.Id),
                        TotalComments = await _commentRepository.CountAsync(c => c.PublicationId == p.Id && c.IsActive),
                        TotalViews = 0, // TODO: Implement views tracking
                        TotalSaved = await _savedPublicationRepository.CountAsync(s => s.PublicationId == p.Id),
                        // Check if current user has interacted (if userId is provided)
                        UserHasReacted = userId.HasValue ? await _reactionRepository.ExistsAsync(r => r.PublicationId == p.Id && r.UserId == userId.Value) : false,
                        UserHasSaved = userId.HasValue ? await _savedPublicationRepository.ExistsAsync(s => s.PublicationId == p.Id && s.UserId == userId.Value) : false
                    });
                }

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
                    IsActive = true,
                    CategoryId = dto.CategoriaId,
                    Priority = dto.Prioridad ?? "Normal",
                    IsPinned = dto.Fijada,
                    Latitude = dto.Latitud.HasValue ? (decimal?)dto.Latitud.Value : null,
                    Longitude = dto.Longitud.HasValue ? (decimal?)dto.Longitud.Value : null
                };

                var createdPublication = await _publicationRepository.AddAsync(publication);

                // Agregar etiquetas si se proporcionaron
                if (!string.IsNullOrWhiteSpace(dto.EtiquetasIds))
                {
                    var etiquetasIds = dto.EtiquetasIds
                        .Split(',')
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Select(id => int.Parse(id.Trim()))
                        .ToList();

                    foreach (var tagId in etiquetasIds)
                    {
                        var tagRelation = new PublicationTagRelation
                        {
                            PublicationId = createdPublication.Id,
                            TagId = tagId
                        };
                        await _tagRelationRepository.AddAsync(tagRelation);
                    }
                }

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
        public async Task<IActionResult> GetPublicationById(int id, [FromQuery] int? userId = null)
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
                    },
                    // Calculate interaction counters
                    TotalReactions = await _reactionRepository.CountAsync(r => r.PublicationId == id),
                    TotalComments = await _commentRepository.CountAsync(c => c.PublicationId == id && c.IsActive),
                    TotalViews = 0, // TODO: Implement views tracking
                    TotalSaved = await _savedPublicationRepository.CountAsync(s => s.PublicationId == id),
                    // Check if current user has interacted (if userId is provided)
                    UserHasReacted = userId.HasValue ? await _reactionRepository.ExistsAsync(r => r.PublicationId == id && r.UserId == userId.Value) : false,
                    UserHasSaved = userId.HasValue ? await _savedPublicationRepository.ExistsAsync(s => s.PublicationId == id && s.UserId == userId.Value) : false
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

        // ==================== NUEVOS ENDPOINTS - FEED Y FILTROS ====================

        /// <summary>
        /// Get feed ordered by priority (Urgente > Alta > Normal > Baja)
        /// Pinned publications appear first
        /// </summary>
        [HttpGet]
        [Route("feed")]
        public async Task<IActionResult> GetFeed(
            [FromQuery] int? ciudadId = null,
            [FromQuery] int? categoriaId = null,
            [FromQuery] int? userId = null,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            try
            {
                var publications = await _publicationRepository.GetAllWithIncludesAsync(
                    p => p.User,
                    p => p.User.Role,
                    p => p.Category,
                    p => p.City,
                    p => p.Department
                );

                // Apply filters
                var filtered = publications.Where(p => p.IsActive && p.IsPublished);

                if (ciudadId.HasValue)
                {
                    filtered = filtered.Where(p => p.CityId == ciudadId.Value);
                }

                if (categoriaId.HasValue)
                {
                    filtered = filtered.Where(p => p.CategoryId == categoriaId.Value);
                }

                // Order by pinned first, then by priority, then by date
                var priorityOrder = new Dictionary<string, int>
                {
                    { "Urgente", 4 },
                    { "Alta", 3 },
                    { "Normal", 2 },
                    { "Baja", 1 }
                };

                var ordered = filtered
                    .OrderByDescending(p => p.IsPinned)
                    .ThenByDescending(p => priorityOrder.ContainsKey(p.Priority) ? priorityOrder[p.Priority] : 0)
                    .ThenByDescending(p => p.CreatedAt)
                    .Skip(offset)
                    .Take(limit)
                    .ToList();

                // Build response with counters
                var response = new List<PublicationResponseDto>();

                foreach (var p in ordered)
                {
                    response.Add(new PublicationResponseDto
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
                        },
                        TotalReactions = await _reactionRepository.CountAsync(r => r.PublicationId == p.Id),
                        TotalComments = await _commentRepository.CountAsync(c => c.PublicationId == p.Id && c.IsActive),
                        TotalViews = await _viewRepository.CountAsync(v => v.PublicationId == p.Id),
                        TotalSaved = await _savedPublicationRepository.CountAsync(s => s.PublicationId == p.Id),
                        UserHasReacted = userId.HasValue ? await _reactionRepository.ExistsAsync(r => r.PublicationId == p.Id && r.UserId == userId.Value) : false,
                        UserHasSaved = userId.HasValue ? await _savedPublicationRepository.ExistsAsync(s => s.PublicationId == p.Id && s.UserId == userId.Value) : false
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el feed", error = ex.Message });
            }
        }

        /// <summary>
        /// Get publications by category
        /// </summary>
        [HttpGet]
        [Route("category/{categoryId}")]
        public async Task<IActionResult> GetPublicationsByCategory(int categoryId, [FromQuery] int? userId = null)
        {
            try
            {
                var publications = await _publicationRepository.GetAllWithIncludesAsync(
                    p => p.User,
                    p => p.User.Role,
                    p => p.Category
                );

                var filtered = publications
                    .Where(p => p.IsActive && p.IsPublished && p.CategoryId == categoryId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                var response = new List<PublicationResponseDto>();

                foreach (var p in filtered)
                {
                    response.Add(new PublicationResponseDto
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
                        },
                        TotalReactions = await _reactionRepository.CountAsync(r => r.PublicationId == p.Id),
                        TotalComments = await _commentRepository.CountAsync(c => c.PublicationId == p.Id && c.IsActive),
                        TotalViews = await _viewRepository.CountAsync(v => v.PublicationId == p.Id),
                        TotalSaved = await _savedPublicationRepository.CountAsync(s => s.PublicationId == p.Id),
                        UserHasReacted = userId.HasValue ? await _reactionRepository.ExistsAsync(r => r.PublicationId == p.Id && r.UserId == userId.Value) : false,
                        UserHasSaved = userId.HasValue ? await _savedPublicationRepository.ExistsAsync(s => s.PublicationId == p.Id && s.UserId == userId.Value) : false
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener publicaciones por categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Get only urgent/alert publications
        /// </summary>
        [HttpGet]
        [Route("urgent")]
        public async Task<IActionResult> GetUrgentPublications([FromQuery] int? userId = null)
        {
            try
            {
                var publications = await _publicationRepository.GetAllWithIncludesAsync(
                    p => p.User,
                    p => p.User.Role
                );

                var urgent = publications
                    .Where(p => p.IsActive && p.IsPublished && p.Priority == "Urgente")
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                var response = new List<PublicationResponseDto>();

                foreach (var p in urgent)
                {
                    response.Add(new PublicationResponseDto
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
                        },
                        TotalReactions = await _reactionRepository.CountAsync(r => r.PublicationId == p.Id),
                        TotalComments = await _commentRepository.CountAsync(c => c.PublicationId == p.Id && c.IsActive),
                        TotalViews = await _viewRepository.CountAsync(v => v.PublicationId == p.Id),
                        TotalSaved = await _savedPublicationRepository.CountAsync(s => s.PublicationId == p.Id),
                        UserHasReacted = userId.HasValue ? await _reactionRepository.ExistsAsync(r => r.PublicationId == p.Id && r.UserId == userId.Value) : false,
                        UserHasSaved = userId.HasValue ? await _savedPublicationRepository.ExistsAsync(s => s.PublicationId == p.Id && s.UserId == userId.Value) : false
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener publicaciones urgentes", error = ex.Message });
            }
        }

        /// <summary>
        /// Get pinned publications
        /// </summary>
        [HttpGet]
        [Route("pinned")]
        public async Task<IActionResult> GetPinnedPublications([FromQuery] int? userId = null)
        {
            try
            {
                var publications = await _publicationRepository.GetAllWithIncludesAsync(
                    p => p.User,
                    p => p.User.Role
                );

                var pinned = publications
                    .Where(p => p.IsActive && p.IsPublished && p.IsPinned)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                var response = new List<PublicationResponseDto>();

                foreach (var p in pinned)
                {
                    response.Add(new PublicationResponseDto
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
                        },
                        TotalReactions = await _reactionRepository.CountAsync(r => r.PublicationId == p.Id),
                        TotalComments = await _commentRepository.CountAsync(c => c.PublicationId == p.Id && c.IsActive),
                        TotalViews = await _viewRepository.CountAsync(v => v.PublicationId == p.Id),
                        TotalSaved = await _savedPublicationRepository.CountAsync(s => s.PublicationId == p.Id),
                        UserHasReacted = userId.HasValue ? await _reactionRepository.ExistsAsync(r => r.PublicationId == p.Id && r.UserId == userId.Value) : false,
                        UserHasSaved = userId.HasValue ? await _savedPublicationRepository.ExistsAsync(s => s.PublicationId == p.Id && s.UserId == userId.Value) : false
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener publicaciones fijadas", error = ex.Message });
            }
        }

        /// <summary>
        /// Get publications near a location (using Haversine formula)
        /// </summary>
        [HttpGet]
        [Route("nearby")]
        public async Task<IActionResult> GetNearbyPublications(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radiusKm = 10.0,
            [FromQuery] int? userId = null)
        {
            try
            {
                var publications = await _publicationRepository.GetAllWithIncludesAsync(
                    p => p.User,
                    p => p.User.Role
                );

                var nearby = publications
                    .Where(p => p.IsActive && p.IsPublished && p.Latitude.HasValue && p.Longitude.HasValue)
                    .ToList()
                    .Where(p => CalculateDistance(lat, lng, (double)p.Latitude!, (double)p.Longitude!) <= radiusKm)
                    .OrderBy(p => CalculateDistance(lat, lng, (double)p.Latitude!, (double)p.Longitude!))
                    .ToList();

                var response = new List<PublicationResponseDto>();

                foreach (var p in nearby)
                {
                    response.Add(new PublicationResponseDto
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
                        },
                        TotalReactions = await _reactionRepository.CountAsync(r => r.PublicationId == p.Id),
                        TotalComments = await _commentRepository.CountAsync(c => c.PublicationId == p.Id && c.IsActive),
                        TotalViews = await _viewRepository.CountAsync(v => v.PublicationId == p.Id),
                        TotalSaved = await _savedPublicationRepository.CountAsync(s => s.PublicationId == p.Id),
                        UserHasReacted = userId.HasValue ? await _reactionRepository.ExistsAsync(r => r.PublicationId == p.Id && r.UserId == userId.Value) : false,
                        UserHasSaved = userId.HasValue ? await _savedPublicationRepository.ExistsAsync(s => s.PublicationId == p.Id && s.UserId == userId.Value) : false
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener publicaciones cercanas", error = ex.Message });
            }
        }

        /// <summary>
        /// Get publications by tag
        /// </summary>
        [HttpGet]
        [Route("tag/{tagId}")]
        public async Task<IActionResult> GetPublicationsByTag(int tagId, [FromQuery] int? userId = null)
        {
            try
            {
                var tagRelations = await _tagRelationRepository.GetAllWithIncludesAsync(
                    tr => tr.Publication,
                    tr => tr.Publication.User,
                    tr => tr.Publication.User.Role
                );

                var filtered = tagRelations
                    .Where(tr => tr.TagId == tagId && tr.Publication.IsActive && tr.Publication.IsPublished)
                    .Select(tr => tr.Publication)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                var response = new List<PublicationResponseDto>();

                foreach (var p in filtered)
                {
                    response.Add(new PublicationResponseDto
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
                        },
                        TotalReactions = await _reactionRepository.CountAsync(r => r.PublicationId == p.Id),
                        TotalComments = await _commentRepository.CountAsync(c => c.PublicationId == p.Id && c.IsActive),
                        TotalViews = await _viewRepository.CountAsync(v => v.PublicationId == p.Id),
                        TotalSaved = await _savedPublicationRepository.CountAsync(s => s.PublicationId == p.Id),
                        UserHasReacted = userId.HasValue ? await _reactionRepository.ExistsAsync(r => r.PublicationId == p.Id && r.UserId == userId.Value) : false,
                        UserHasSaved = userId.HasValue ? await _savedPublicationRepository.ExistsAsync(s => s.PublicationId == p.Id && s.UserId == userId.Value) : false
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener publicaciones por etiqueta", error = ex.Message });
            }
        }

        /// <summary>
        /// Search publications
        /// </summary>
        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchPublications(
            [FromQuery] string query,
            [FromQuery] int? categoriaId = null,
            [FromQuery] int? userId = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "El término de búsqueda no puede estar vacío" });
            }

            try
            {
                var publications = await _publicationRepository.GetAllWithIncludesAsync(
                    p => p.User,
                    p => p.User.Role,
                    p => p.Category
                );

                var filtered = publications
                    .Where(p => p.IsActive && p.IsPublished &&
                        (p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                         p.Description.Contains(query, StringComparison.OrdinalIgnoreCase)));

                if (categoriaId.HasValue)
                {
                    filtered = filtered.Where(p => p.CategoryId == categoriaId.Value);
                }

                var results = filtered
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                var response = new List<PublicationResponseDto>();

                foreach (var p in results)
                {
                    response.Add(new PublicationResponseDto
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
                        },
                        TotalReactions = await _reactionRepository.CountAsync(r => r.PublicationId == p.Id),
                        TotalComments = await _commentRepository.CountAsync(c => c.PublicationId == p.Id && c.IsActive),
                        TotalViews = await _viewRepository.CountAsync(v => v.PublicationId == p.Id),
                        TotalSaved = await _savedPublicationRepository.CountAsync(s => s.PublicationId == p.Id),
                        UserHasReacted = userId.HasValue ? await _reactionRepository.ExistsAsync(r => r.PublicationId == p.Id && r.UserId == userId.Value) : false,
                        UserHasSaved = userId.HasValue ? await _savedPublicationRepository.ExistsAsync(s => s.PublicationId == p.Id && s.UserId == userId.Value) : false
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar publicaciones", error = ex.Message });
            }
        }

        /// <summary>
        /// Register a view on a publication
        /// </summary>
        [HttpPost]
        [Route("{id}/view")]
        public async Task<IActionResult> RegisterView(int id, [FromBody] RegisterViewDto dto)
        {
            try
            {
                var publication = await _publicationRepository.GetByIdAsync(id);
                if (publication == null)
                {
                    return NotFound(new { message = "Publicación no encontrada" });
                }

                // Check if user already viewed this publication (to avoid duplicate views)
                var existingView = await _viewRepository.FindAsync(v =>
                    v.PublicationId == id && v.UserId == dto.UserId);

                if (!existingView.Any())
                {
                    var view = new PublicationView
                    {
                        PublicationId = id,
                        UserId = dto.UserId,
                        ViewedAt = DateTime.Now,
                        ReadingTimeSeconds = dto.ReadTimeSeconds
                    };
                    await _viewRepository.AddAsync(view);
                }

                return Ok(new { message = "Vista registrada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al registrar vista", error = ex.Message });
            }
        }

        /// <summary>
        /// Get trending publications (most popular in last N days)
        /// </summary>
        [HttpGet]
        [Route("trending")]
        public async Task<IActionResult> GetTrendingPublications(
            [FromQuery] int limit = 10,
            [FromQuery] int days = 7,
            [FromQuery] int? userId = null)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-days);
                var publications = await _publicationRepository.GetAllWithIncludesAsync(
                    p => p.User,
                    p => p.User.Role
                );

                var recent = publications
                    .Where(p => p.IsActive && p.IsPublished && p.CreatedAt >= cutoffDate)
                    .ToList();

                // Calculate popularity score (reactions + comments * 2 + saves * 3)
                var trending = new List<(Publication pub, int score)>();

                foreach (var p in recent)
                {
                    var reactions = await _reactionRepository.CountAsync(r => r.PublicationId == p.Id);
                    var comments = await _commentRepository.CountAsync(c => c.PublicationId == p.Id && c.IsActive);
                    var saves = await _savedPublicationRepository.CountAsync(s => s.PublicationId == p.Id);
                    var score = reactions + (comments * 2) + (saves * 3);
                    trending.Add((p, score));
                }

                var topTrending = trending
                    .OrderByDescending(t => t.score)
                    .Take(limit)
                    .ToList();

                var response = new List<PublicationResponseDto>();

                foreach (var (p, _) in topTrending)
                {
                    response.Add(new PublicationResponseDto
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
                        },
                        TotalReactions = await _reactionRepository.CountAsync(r => r.PublicationId == p.Id),
                        TotalComments = await _commentRepository.CountAsync(c => c.PublicationId == p.Id && c.IsActive),
                        TotalViews = await _viewRepository.CountAsync(v => v.PublicationId == p.Id),
                        TotalSaved = await _savedPublicationRepository.CountAsync(s => s.PublicationId == p.Id),
                        UserHasReacted = userId.HasValue ? await _reactionRepository.ExistsAsync(r => r.PublicationId == p.Id && r.UserId == userId.Value) : false,
                        UserHasSaved = userId.HasValue ? await _savedPublicationRepository.ExistsAsync(s => s.PublicationId == p.Id && s.UserId == userId.Value) : false
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener publicaciones trending", error = ex.Message });
            }
        }

        // ==================== HELPER METHODS ====================

        /// <summary>
        /// Calculate distance between two coordinates using Haversine formula
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
