using Backend_App_Dengue.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    /// <summary>
    /// Controlador para gestión de imágenes almacenadas en MongoDB GridFS
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ImageController : ControllerBase
    {
        ConexionMongo mongo = new ConexionMongo();

        /// <summary>
        /// Obtiene una imagen desde MongoDB GridFS por su ID
        /// </summary>
        /// <param name="id">ID de la imagen en MongoDB</param>
        /// <returns>Archivo de imagen con el tipo MIME detectado automáticamente</returns>
        /// <response code="200">Imagen retornada exitosamente</response>
        /// <response code="400">ID de imagen requerido o formato inválido</response>
        /// <response code="404">Imagen no encontrada</response>
        /// <response code="500">Error al obtener la imagen</response>
        /// <remarks>
        /// Detecta automáticamente el tipo de imagen (PNG, JPEG, GIF, WebP) usando magic bytes.
        /// La imagen se almacena en formato Base64 en MongoDB y se convierte a bytes para la respuesta.
        /// </remarks>
        [HttpGet("getImage/{id}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult GetImage(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "El ID de la imagen es requerido" });
            }

            try
            {
                var img = mongo.GetImage(id);
                if (img == null || string.IsNullOrEmpty(img.Imagen))
                {
                    return NotFound(new { message = "Imagen no encontrada" });
                }

                var imageBytes = Convert.FromBase64String(img.Imagen);

                // Detectar tipo de imagen por magic bytes
                string contentType = "image/jpeg"; // default

                if (imageBytes.Length >= 2)
                {
                    // PNG: 89 50 4E 47
                    if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50)
                    {
                        contentType = "image/png";
                    }
                    // JPEG: FF D8 FF
                    else if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
                    {
                        contentType = "image/jpeg";
                    }
                    // GIF: 47 49 46
                    else if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49)
                    {
                        contentType = "image/gif";
                    }
                    // WebP: 52 49 46 46
                    else if (imageBytes.Length >= 12 && imageBytes[0] == 0x52 && imageBytes[1] == 0x49)
                    {
                        contentType = "image/webp";
                    }
                }

                return File(imageBytes, contentType);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Formato de imagen inválido" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la imagen", error = ex.Message });
            }
        }
    }
}
