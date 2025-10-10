using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HospitalController : ControllerBase
    {
        internal Connection cn = new Connection();
        internal ConexionMongo mongo = new ConexionMongo();

        [HttpGet]
        [Route("filterHospitals")]
        public IActionResult filterHospitals([FromQuery] string name)
        {
            try
            {
                string[] aux = { name ?? "" };
                string[] parametros = { "nombre" };
                DataTable h = cn.ProcedimientosSelect(parametros, "FiltrarHospital", aux);
                List<HospitalModel> hospitals = h.DataTableToList<HospitalModel>();

                if (hospitals == null)
                {
                    return Ok(new List<HospitalModel>());
                }

                return Ok(hospitals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al filtrar hospitales", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("getHospitals")]
        public IActionResult getHospitals()
        {
            try
            {
                DataTable h = cn.ProcedimientosSelect(null, "ListarHospi", null);
                List<HospitalModel> hospitals = h.DataTableToList<HospitalModel>();

                if (hospitals == null)
                {
                    return Ok(new List<HospitalModel>());
                }

                return Ok(hospitals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los hospitales", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("getHospitalByCity")]
        public IActionResult getHospitalByCity([FromQuery] string filtro)
        {
            if (string.IsNullOrEmpty(filtro))
            {
                return BadRequest(new { message = "El filtro de ciudad es requerido" });
            }

            try
            {
                string[] aux = { filtro };
                string[] parametros = { "filtro" };
                DataTable h = cn.ProcedimientosSelect(parametros, "ListarHospital", aux);
                List<HospitalModel> hospitals = h.DataTableToList<HospitalModel>();

                if (hospitals == null)
                {
                    return Ok(new List<HospitalModel>());
                }

                return Ok(hospitals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener hospitales por ciudad", error = ex.Message });
            }
        }

        // HU-009: Obtener Hospital por ID
        [HttpGet]
        [Route("getHospitalById/{id}")]
        public IActionResult GetHospitalById(int id)
        {
            try
            {
                string[] parametros = { "idh" };
                string[] valores = { id.ToString() };

                DataTable h = cn.ProcedimientosSelect(parametros, "ObtenerHospital", valores);
                List<HospitalModel> hospitals = h.DataTableToList<HospitalModel>();

                if (hospitals == null || hospitals.Count == 0)
                {
                    return NotFound(new { message = "Hospital no encontrado" });
                }

                return Ok(hospitals[0]);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el hospital", error = ex.Message });
            }
        }

        // HU-009: Crear Hospital
        [HttpPost]
        [Route("createHospital")]
        public async Task<IActionResult> CreateHospital([FromForm] CreateHospitalDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos del hospital son requeridos" });
            }

            string? imagenId = null;

            try
            {
                // Si hay imagen, subirla a MongoDB
                if (dto.Imagen != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        await dto.Imagen.CopyToAsync(stream);
                        var imagenModel = new ImagenModel
                        {
                            Imagen = Convert.ToBase64String(stream.ToArray())
                        };
                        imagenId = mongo.UploadImage(imagenModel);
                    }
                }

                // Insertar hospital en MySQL
                string[] parametros = { "nombre", "direccion", "lat", "lon", "muni", "imagen" };
                string[] valores = {
                    dto.Nombre,
                    dto.Direccion,
                    dto.Latitud,
                    dto.Longitud,
                    dto.IdMunicipio.ToString(),
                    imagenId ?? ""
                };

                cn.procedimientosInEd(parametros, "CrearHospital", valores);

                return Ok(new { message = "Hospital creado con éxito", imagenId = imagenId });
            }
            catch (Exception ex)
            {
                // Si falla después de subir imagen, eliminarla
                if (!string.IsNullOrEmpty(imagenId))
                {
                    try { mongo.DeleteImage(imagenId); } catch { }
                }

                return StatusCode(500, new { message = "Error al crear el hospital", error = ex.Message });
            }
        }

        // HU-009: Actualizar Hospital
        [HttpPut]
        [Route("updateHospital/{id}")]
        public IActionResult UpdateHospital(int id, [FromBody] UpdateHospitalDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos del hospital son requeridos" });
            }

            try
            {
                string[] parametros = { "idh", "nombre", "imagenN" };
                string[] valores = {
                    id.ToString(),
                    dto.Nombre ?? "",
                    dto.ImagenId ?? ""
                };

                cn.procedimientosInEd(parametros, "EditarHospital", valores);

                return Ok(new { message = "Hospital actualizado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el hospital", error = ex.Message });
            }
        }

        // HU-009: Eliminar Hospital (Soft Delete)
        [HttpDelete]
        [Route("deleteHospital/{id}")]
        public IActionResult DeleteHospital(int id)
        {
            try
            {
                string[] parametros = { "idh" };
                string[] valores = { id.ToString() };

                cn.procedimientosInEd(parametros, "EliminarHospital", valores);

                return Ok(new { message = "Hospital eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el hospital", error = ex.Message });
            }
        }
    }
}
