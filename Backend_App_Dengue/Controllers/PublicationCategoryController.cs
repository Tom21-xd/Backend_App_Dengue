using Backend_App_Dengue.Data.Repository;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PublicationCategoryController : ControllerBase
    {
        private readonly PublicationCategoryRepository _repository;

        public PublicationCategoryController(PublicationCategoryRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Obtener todas las categorías de publicaciones
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<PublicationCategory>>> GetAllCategories()
        {
            try
            {
                var categories = await _repository.GetAllAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener categorías", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener categoría por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PublicationCategory>> GetCategoryById(int id)
        {
            try
            {
                var category = await _repository.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Crear nueva categoría (admin)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PublicationCategory>> CreateCategory([FromBody] PublicationCategory category)
        {
            try
            {
                var created = await _repository.AddAsync(category);
                return CreatedAtAction(nameof(GetCategoryById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar categoría
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCategory(int id, [FromBody] PublicationCategory category)
        {
            try
            {
                if (id != category.Id)
                {
                    return BadRequest(new { message = "ID de categoría no coincide" });
                }

                var success = await _repository.UpdateAsync(category);
                if (!success)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                return Ok(new { message = "Categoría actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar categoría
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                var success = await _repository.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                return Ok(new { message = "Categoría eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar categoría", error = ex.Message });
            }
        }
    }
}
