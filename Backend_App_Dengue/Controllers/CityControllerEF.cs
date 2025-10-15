using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("City")]
    [ApiController]
    public class CityControllerEF : ControllerBase
    {
        private readonly IRepository<City> _cityRepository;

        public CityControllerEF(IRepository<City> cityRepository)
        {
            _cityRepository = cityRepository;
        }

        /// <summary>
        /// Get all cities
        /// </summary>
        [HttpGet]
        [Route("getCities")]
        public async Task<IActionResult> GetCities([FromQuery] string? filter)
        {
            try
            {
                IEnumerable<City> cities;

                if (!string.IsNullOrWhiteSpace(filter) && int.TryParse(filter, out int departmentId))
                {
                    // Filter cities by department ID
                    cities = await _cityRepository.FindAsync(c => c.DepartmentId == departmentId);
                }
                else
                {
                    // Get all cities
                    cities = await _cityRepository.GetAllAsync();
                }

                return Ok(cities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener ciudades", error = ex.Message });
            }
        }

        /// <summary>
        /// Get city by ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetCityById(int id)
        {
            try
            {
                var city = await _cityRepository.GetByIdAsync(id);

                if (city == null)
                {
                    return NotFound(new { message = "Ciudad no encontrada" });
                }

                return Ok(city);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la ciudad", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new city
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCity([FromBody] City city)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(city.Name))
                {
                    return BadRequest(new { message = "El nombre de la ciudad es requerido" });
                }

                if (city.DepartmentId <= 0)
                {
                    return BadRequest(new { message = "El departamento es requerido" });
                }

                var createdCity = await _cityRepository.AddAsync(city);
                return CreatedAtAction(nameof(GetCityById), new { id = createdCity.Id }, createdCity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear la ciudad", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing city
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateCity(int id, [FromBody] City city)
        {
            try
            {
                var existingCity = await _cityRepository.GetByIdAsync(id);

                if (existingCity == null)
                {
                    return NotFound(new { message = "Ciudad no encontrada" });
                }

                existingCity.Name = city.Name;
                existingCity.DepartmentId = city.DepartmentId;
                existingCity.IsActive = city.IsActive;

                await _cityRepository.UpdateAsync(existingCity);
                return Ok(new { message = "Ciudad actualizada con éxito", city = existingCity });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la ciudad", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a city
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            try
            {
                var city = await _cityRepository.GetByIdAsync(id);

                if (city == null)
                {
                    return NotFound(new { message = "Ciudad no encontrada" });
                }

                await _cityRepository.DeleteAsync(city);
                return Ok(new { message = "Ciudad eliminada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la ciudad", error = ex.Message });
            }
        }
    }
}
