using Backend_App_Dengue.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;

namespace Backend_App_Dengue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        ConexionMongo mongo = new ConexionMongo();

        [HttpGet("getImage/{id}")]
        public IActionResult GetImage(string id)
        {
            var img = mongo.GetImage(id);
            if (img == null || img.Imagen == null)
                return NotFound();

            var imageBytes = Convert.FromBase64String(img.Imagen);
            return File(imageBytes, "image/jpeg");
        }
    }
}
