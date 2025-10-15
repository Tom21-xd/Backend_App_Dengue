using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Backend_App_Dengue.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        Connection cn = new Connection();
        ConexionMongo _conexionMongo = new ConexionMongo();
        private readonly FCMService _fcmService = new FCMService();

        [HttpGet]
        [Route("getPublications")]
        public IActionResult getPublications()
        {
            try
            {
                DataTable dt = cn.ProcedimientosSelect(null, "ListarPublicaciones", null);
                List<PublicationModel> lista = dt.DataTableToList<PublicationModel>();

                if (lista == null)
                {
                    return Ok(new List<PublicationModel>());
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las publicaciones", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("getPublication")]
        [ProducesResponseType(typeof(List<PublicationModel>), 200)]
        public IActionResult getPublication([FromQuery] string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                return BadRequest(new { message = "El nombre no puede estar vacío" });
            }

            try
            {
                string[] parametros = { "nombre" };
                string[] valores = { nombre };

                DataTable dt = cn.ProcedimientosSelect(parametros, "Filtrarpublicacion", valores);
                List<PublicationModel> lista = dt.DataTableToList<PublicationModel>();

                if (lista == null)
                {
                    return Ok(new List<PublicationModel>());
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar la publicación", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("createPublication")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> InsertarPublicacion([FromForm] CreatePublicationModelDto createPublicationModel)
        {
            if (createPublicationModel == null)
            {
                return BadRequest(new { message = "Faltan datos en la solicitud" });
            }

            if (createPublicationModel.imagen == null)
            {
                return BadRequest(new { message = "La imagen es requerida" });
            }

            string imagenId = null;

            try
            {
                // Subir imagen a MongoDB
                using (var stream = new MemoryStream())
                {
                    await createPublicationModel.imagen.CopyToAsync(stream);
                    var imagenModel = new ImagenModel
                    {
                        Imagen = Convert.ToBase64String(stream.ToArray())
                    };
                    imagenId = _conexionMongo.UploadImage(imagenModel);
                }

                // Insertar publicación en MySQL
                string[] parametros = { "titulo", "idi", "descri", "usua" };
                string[] valores = {
                    createPublicationModel.Titulo,
                    imagenId,
                    createPublicationModel.Descripcion,
                    createPublicationModel.UsuarioId
                };

                cn.procedimientosInEd(parametros, "CrearPublicacion", valores);

                // Enviar notificación push a todos los usuarios
                try
                {
                    DataTable dtTokens = cn.ProcedimientosSelect(null, "ObtenerTodosLosFCMTokens", null);
                    List<string> tokens = new List<string>();

                    foreach (DataRow row in dtTokens.Rows)
                    {
                        tokens.Add(row["FCM_TOKEN"].ToString());
                    }

                    if (tokens.Count > 0)
                    {
                        var data = new Dictionary<string, string>
                        {
                            { "type", "new_publication" },
                            { "titulo", createPublicationModel.Titulo }
                        };

                        // Limitar el mensaje a 100 caracteres
                        string descripcionCorta = createPublicationModel.Descripcion.Length > 100
                            ? createPublicationModel.Descripcion.Substring(0, 100) + "..."
                            : createPublicationModel.Descripcion;

                        await _fcmService.SendNotificationToMultipleDevices(
                            tokens,
                            $"Nueva Publicación: {createPublicationModel.Titulo}",
                            descripcionCorta,
                            data
                        );
                    }
                }
                catch (Exception fcmEx)
                {
                    Console.WriteLine($"Error al enviar notificación push: {fcmEx.Message}");
                    // No fallar la creación de la publicación si falla la notificación
                }

                return Ok(new { message = "Publicación creada con éxito", imagenId = imagenId });
            }
            catch (Exception ex)
            {
                // Si falla después de subir la imagen, intentar eliminarla
                if (!string.IsNullOrEmpty(imagenId))
                {
                    try
                    {
                        _conexionMongo.DeleteImage(imagenId);
                    }
                    catch
                    {
                        // Log: No se pudo eliminar la imagen huérfana
                    }
                }

                return StatusCode(500, new { message = "Error al insertar la publicación", error = ex.Message });
            }
        }

        // HU-007: Obtener Publicación por ID
        [HttpGet]
        [Route("getPublicationById/{id}")]
        public IActionResult GetPublicationById(int id)
        {
            try
            {
                string[] parametros = { "idp" };
                string[] valores = { id.ToString() };

                DataTable dt = cn.ProcedimientosSelect(parametros, "ObtenerPublicacion", valores);
                List<PublicationModel> lista = dt.DataTableToList<PublicationModel>();

                if (lista == null || lista.Count == 0)
                {
                    return NotFound(new { message = "Publicación no encontrada" });
                }

                return Ok(lista[0]);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la publicación", error = ex.Message });
            }
        }

        // HU-007: Actualizar Publicación
        [HttpPut]
        [Route("updatePublication/{id}")]
        public IActionResult UpdatePublication(int id, [FromBody] UpdatePublicationDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos de la publicación son requeridos" });
            }

            try
            {
                string[] parametros = { "idp", "titulo", "descri" };
                string[] valores = {
                    id.ToString(),
                    dto.Titulo ?? "",
                    dto.Descripcion ?? ""
                };

                cn.procedimientosInEd(parametros, "EditarPublicacion", valores);

                return Ok(new { message = "Publicación actualizada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la publicación", error = ex.Message });
            }
        }

        // HU-007: Eliminar Publicación
        [HttpDelete]
        [Route("deletePublication/{id}")]
        public IActionResult DeletePublication(int id)
        {
            try
            {
                string[] parametros = { "idp" };
                string[] valores = { id.ToString() };

                cn.procedimientosInEd(parametros, "EliminarPublicacion", valores);

                return Ok(new { message = "Publicación eliminada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la publicación", error = ex.Message });
            }
        }
    }
}
