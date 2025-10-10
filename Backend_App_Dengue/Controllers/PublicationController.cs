using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        Connection cn = new Connection();
        ConexionMongo _conexionMongo = new ConexionMongo();

        [HttpGet]
        [Route("getPublications")]
        public async Task<IActionResult> getPublications()
        {
            DataTable dt = cn.ProcedimientosSelect(null, "ListarPublicaciones", null);
            List<PublicationModel> lista = dt.DataTableToList<PublicationModel>();
            return Ok(lista);
        }

        [HttpGet]
        [Route("getPublication")]
        [ProducesResponseType(typeof(List<PublicationModel>), 200)]
        public async Task<IActionResult> getPublication([FromQuery] string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                return BadRequest("El nombre no puede estar vacío.");
            }

            string[] parametros = { "nombre" };
            string[] valores = { nombre };

            DataTable dt = cn.ProcedimientosSelect(parametros, "Filtrarpublicacion", valores);

            List<PublicationModel> lista = dt.DataTableToList<PublicationModel>();

            return Ok(lista);
        }

        [HttpPost]
        [Route("createPublication")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> InsertarPublicacion([FromForm] CreatePublicationModelDto createPublicationModel)
        {
            if (createPublicationModel == null)
            {
                return BadRequest("Faltan datos en la solicitud.");
            }

            string imagenId;
            using (var stream = new MemoryStream())
            {
                await createPublicationModel.imagen.CopyToAsync(stream);
                var imagenModel = new ImagenModel
                {
                    Imagen = Convert.ToBase64String(stream.ToArray())
                };
                imagenId = _conexionMongo.UploadImage(imagenModel);
            }
            string[] parametros = { "titulo", "idi", "descri", "usua" };
            string[] valores = { createPublicationModel.Titulo, imagenId, createPublicationModel.Descripcion, createPublicationModel.UsuarioId };
            try
            {
                cn.procedimientosInEd(parametros, "CrearPublicacion", valores);
                return Ok("Publicacion Creada");
            }
            catch (Exception ex)
            {
                return BadRequest("Error al insertar la publicación: " + ex.Message);

            }

        }
    }
}