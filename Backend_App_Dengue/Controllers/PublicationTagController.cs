using Backend_App_Dengue.Data.Repository;
using Backend_App_Dengue.Model;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PublicationTagController : ControllerBase
    {
        private readonly PublicationTagRepository _repository;

        public PublicationTagController(PublicationTagRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Obtener todas las etiquetas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<PublicationTag>>> GetAllTags()
        {
            try
            {
                var tags = await _repository.GetAllAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener etiquetas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener etiqueta por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PublicationTag>> GetTagById(int id)
        {
            try
            {
                var tag = await _repository.GetByIdAsync(id);
                if (tag == null)
                {
                    return NotFound(new { message = "Etiqueta no encontrada" });
                }
                return Ok(tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener etiqueta", error = ex.Message });
            }
        }

        /// <summary>
        /// Buscar etiquetas por nombre
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<PublicationTag>>> SearchTags([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "Query de búsqueda requerido" });
                }

                var tags = await _repository.SearchAsync(query);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar etiquetas", error = ex.Message });
            }
        }

        /// <summary>
        /// Crear nueva etiqueta (admin)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PublicationTag>> CreateTag([FromBody] PublicationTag tag)
        {
            try
            {
                var created = await _repository.AddAsync(tag);
                return CreatedAtAction(nameof(GetTagById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear etiqueta", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar etiqueta
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTag(int id, [FromBody] PublicationTag tag)
        {
            try
            {
                if (id != tag.Id)
                {
                    return BadRequest(new { message = "ID de etiqueta no coincide" });
                }

                var success = await _repository.UpdateAsync(tag);
                if (!success)
                {
                    return NotFound(new { message = "Etiqueta no encontrada" });
                }

                return Ok(new { message = "Etiqueta actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar etiqueta", error = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar etiqueta
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTag(int id)
        {
            try
            {
                var success = await _repository.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Etiqueta no encontrada" });
                }

                return Ok(new { message = "Etiqueta eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar etiqueta", error = ex.Message });
            }
        }
    }
}
