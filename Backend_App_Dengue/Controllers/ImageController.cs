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

                // Retornar como imagen genérica, el navegador determinará el tipo
                return File(imageBytes, "image/jpeg");
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
