using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("Hospital")]
    [ApiController]
    public class HospitalControllerEF : ControllerBase
    {
        private readonly IRepository<Hospital> _hospitalRepository;
        private readonly ConexionMongo _mongo;

        public HospitalControllerEF(IRepository<Hospital> hospitalRepository)
        {
            _hospitalRepository = hospitalRepository;
            _mongo = new ConexionMongo();
        }

        /// <summary>
        /// Filter hospitals by name
        /// </summary>
        [HttpGet]
        [Route("filterHospitals")]
        public async Task<IActionResult> FilterHospitals([FromQuery] string name)
        {
            try
            {
                var hospitals = string.IsNullOrWhiteSpace(name)
                    ? await _hospitalRepository.GetAllAsync()
                    : await _hospitalRepository.FindAsync(h => h.Name.Contains(name));

                return Ok(hospitals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al filtrar hospitales", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all hospitals
        /// </summary>
        [HttpGet]
        [Route("getHospitals")]
        public async Task<IActionResult> GetHospitals()
        {
            try
            {
                var hospitals = await _hospitalRepository.GetAllAsync();
                return Ok(hospitals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los hospitales", error = ex.Message });
            }
        }

        /// <summary>
        /// Get hospitals by city
        /// </summary>
        [HttpGet]
        [Route("getHospitalByCity")]
        public async Task<IActionResult> GetHospitalByCity([FromQuery] string filtro)
        {
            if (string.IsNullOrEmpty(filtro))
            {
                return BadRequest(new { message = "El filtro de ciudad es requerido" });
            }

            try
            {
                if (!int.TryParse(filtro, out int cityId))
                {
                    return BadRequest(new { message = "El ID de ciudad debe ser un número válido" });
                }

                var hospitals = await _hospitalRepository.FindAsync(h => h.CityId == cityId);
                return Ok(hospitals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener hospitales por ciudad", error = ex.Message });
            }
        }

        /// <summary>
        /// Get hospital by ID
        /// </summary>
        [HttpGet]
        [Route("getHospitalById/{id}")]
        public async Task<IActionResult> GetHospitalById(int id)
        {
            try
            {
                var hospital = await _hospitalRepository.GetByIdAsync(id);

                if (hospital == null)
                {
                    return NotFound(new { message = "Hospital no encontrado" });
                }

                return Ok(hospital);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el hospital", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new hospital with optional image upload to MongoDB
        /// </summary>
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
                // Si hay imagen, subirla a MongoDB GridFS
                if (dto.Imagen != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        await dto.Imagen.CopyToAsync(stream);
                        var imagenModel = new ImagenModel
                        {
                            Imagen = Convert.ToBase64String(stream.ToArray())
                        };
                        imagenId = _mongo.UploadImage(imagenModel);
                    }
                }

                // Crear hospital en MySQL con EF Core
                var hospital = new Hospital
                {
                    Name = dto.Nombre,
                    Address = dto.Direccion,
                    Latitude = dto.Latitud,
                    Longitude = dto.Longitud,
                    CityId = dto.IdMunicipio,
                    ImageId = imagenId ?? string.Empty,
                    IsActive = true
                };

                var createdHospital = await _hospitalRepository.AddAsync(hospital);

                return Ok(new { message = "Hospital creado con éxito", hospital = createdHospital, imagenId = imagenId });
            }
            catch (Exception ex)
            {
                // Si falla después de subir imagen, eliminarla de MongoDB
                if (!string.IsNullOrEmpty(imagenId))
                {
                    try { _mongo.DeleteImage(imagenId); } catch { }
                }

                return StatusCode(500, new { message = "Error al crear el hospital", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing hospital
        /// </summary>
        [HttpPut]
        [Route("updateHospital/{id}")]
        public async Task<IActionResult> UpdateHospital(int id, [FromBody] UpdateHospitalDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos del hospital son requeridos" });
            }

            try
            {
                var existingHospital = await _hospitalRepository.GetByIdAsync(id);

                if (existingHospital == null)
                {
                    return NotFound(new { message = "Hospital no encontrado" });
                }

                // Actualizar solo los campos proporcionados
                if (!string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    existingHospital.Name = dto.Nombre;
                }

                if (!string.IsNullOrWhiteSpace(dto.ImagenId))
                {
                    existingHospital.ImageId = dto.ImagenId;
                }

                await _hospitalRepository.UpdateAsync(existingHospital);

                return Ok(new { message = "Hospital actualizado con éxito", hospital = existingHospital });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el hospital", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a hospital (soft delete)
        /// </summary>
        [HttpDelete]
        [Route("deleteHospital/{id}")]
        public async Task<IActionResult> DeleteHospital(int id)
        {
            try
            {
                var hospital = await _hospitalRepository.GetByIdAsync(id);

                if (hospital == null)
                {
                    return NotFound(new { message = "Hospital no encontrado" });
                }

                // Soft delete - marcar como inactivo
                hospital.IsActive = false;
                await _hospitalRepository.UpdateAsync(hospital);

                return Ok(new { message = "Hospital eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el hospital", error = ex.Message });
            }
        }
    }
}
