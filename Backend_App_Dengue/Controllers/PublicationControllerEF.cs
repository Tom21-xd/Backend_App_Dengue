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
        private readonly ConexionMongo _conexionMongo;
        private readonly FCMService _fcmService;

        public PublicationControllerEF(
            IRepository<Publication> publicationRepository,
            IRepository<FCMToken> fcmTokenRepository)
        {
            _publicationRepository = publicationRepository;
            _fcmTokenRepository = fcmTokenRepository;
            _conexionMongo = new ConexionMongo();
            _fcmService = new FCMService();
        }

        /// <summary>
        /// Get all publications
        /// </summary>
        [HttpGet]
        [Route("getPublications")]
        public async Task<IActionResult> GetPublications()
        {
            try
            {
                var publications = await _publicationRepository.GetAllAsync();
                return Ok(publications);
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

                        // Limit message to 100 characters
                        string descripcionCorta = dto.Descripcion.Length > 100
                            ? dto.Descripcion.Substring(0, 100) + "..."
                            : dto.Descripcion;

                        await _fcmService.SendNotificationToMultipleDevices(
                            tokens,
                            $"Nueva Publicación: {dto.Titulo}",
                            descripcionCorta,
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
        /// HU-007: Get publication by ID
        /// </summary>
        [HttpGet]
        [Route("getPublicationById/{id}")]
        public async Task<IActionResult> GetPublicationById(int id)
        {
            try
            {
                var publication = await _publicationRepository.GetByIdAsync(id);

                if (publication == null)
                {
                    return NotFound(new { message = "Publicación no encontrada" });
                }

                return Ok(publication);
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
