using Backend_App_Dengue.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        ConexionMongo mongo = new ConexionMongo();

        [HttpGet("getImage/{id}")]
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
