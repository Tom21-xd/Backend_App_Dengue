using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("Genre")]
    [ApiController]
    public class GenreControllerEF : ControllerBase
    {
        private readonly IRepository<Genre> _genreRepository;

        public GenreControllerEF(IRepository<Genre> genreRepository)
        {
            _genreRepository = genreRepository;
        }

        /// <summary>
        /// Get all genres
        /// </summary>
        [HttpGet]
        [Route("getGenres")]
        public async Task<IActionResult> GetGenres()
        {
            try
            {
                var genres = await _genreRepository.GetAllAsync();
                return Ok(genres);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener géneros", error = ex.Message });
            }
        }

        /// <summary>
        /// Get genre by ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetGenreById(int id)
        {
            try
            {
                var genre = await _genreRepository.GetByIdAsync(id);

                if (genre == null)
                {
                    return NotFound(new { message = "Género no encontrado" });
                }

                return Ok(genre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el género", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new genre
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateGenre([FromBody] Genre genre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(genre.Name))
                {
                    return BadRequest(new { message = "El nombre del género es requerido" });
                }

                var createdGenre = await _genreRepository.AddAsync(genre);
                return CreatedAtAction(nameof(GetGenreById), new { id = createdGenre.Id }, createdGenre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el género", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing genre
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] Genre genre)
        {
            try
            {
                var existingGenre = await _genreRepository.GetByIdAsync(id);

                if (existingGenre == null)
                {
                    return NotFound(new { message = "Género no encontrado" });
                }

                existingGenre.Name = genre.Name;
                existingGenre.IsActive = genre.IsActive;

                await _genreRepository.UpdateAsync(existingGenre);
                return Ok(new { message = "Género actualizado con éxito", genre = existingGenre });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el género", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a genre
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            try
            {
                var genre = await _genreRepository.GetByIdAsync(id);

                if (genre == null)
                {
                    return NotFound(new { message = "Género no encontrado" });
                }

                await _genreRepository.DeleteAsync(genre);
                return Ok(new { message = "Género eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el género", error = ex.Message });
            }
        }
    }
}
