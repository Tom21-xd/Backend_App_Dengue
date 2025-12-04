using Backend_App_Dengue.Data;
using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PreventionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ConexionMongo _mongoConnection;

        public PreventionController(AppDbContext context)
        {
            _context = context;
            _mongoConnection = new ConexionMongo();
        }

        // ==================== CATEGORIES ====================

        /// <summary>
        /// Get all active prevention categories with images and items (for mobile app)
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<PreventionCategoryResponseDto>>> GetCategories()
        {
            try
            {
                var categories = await _context.PreventionCategories
                    .Where(c => c.IsActive)
                    .Include(c => c.Images.OrderBy(i => i.DisplayOrder))
                    .Include(c => c.Items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
                        .ThenInclude(i => i.Images.OrderBy(img => img.DisplayOrder))
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                var response = categories.Select(MapCategoryToResponseDto).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las categorías de prevención", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all prevention categories for admin (includes inactive)
        /// </summary>
        [HttpGet("categories/admin")]
        public async Task<ActionResult<List<PreventionCategoryListDto>>> GetCategoriesAdmin()
        {
            try
            {
                var categories = await _context.PreventionCategories
                    .Include(c => c.Images)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Images)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                var response = categories.Select(c => new PreventionCategoryListDto
                {
                    ID_CATEGORIA_PREVENCION = c.Id,
                    NOMBRE_CATEGORIA = c.Name,
                    DESCRIPCION_CATEGORIA = c.Description,
                    ICONO = c.Icon,
                    COLOR = c.Color,
                    ORDEN_VISUALIZACION = c.DisplayOrder,
                    ESTADO_CATEGORIA = c.IsActive,
                    TOTAL_IMAGENES = c.Images.Count,
                    TOTAL_ITEMS = c.Items.Count
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las categorías", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a single category by ID with all images and items
        /// </summary>
        [HttpGet("category/{id}")]
        public async Task<ActionResult<PreventionCategoryResponseDto>> GetCategory(int id)
        {
            try
            {
                var category = await _context.PreventionCategories
                    .Include(c => c.Images.OrderBy(i => i.DisplayOrder))
                    .Include(c => c.Items.OrderBy(i => i.DisplayOrder))
                        .ThenInclude(i => i.Images.OrderBy(img => img.DisplayOrder))
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                return Ok(MapCategoryToResponseDto(category));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new prevention category
        /// </summary>
        [HttpPost("category")]
        public async Task<ActionResult<PreventionCategoryResponseDto>> CreateCategory([FromBody] CreatePreventionCategoryDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var category = new PreventionCategory
                {
                    Name = dto.NOMBRE_CATEGORIA,
                    Description = dto.DESCRIPCION_CATEGORIA,
                    Icon = dto.ICONO,
                    Color = dto.COLOR,
                    DisplayOrder = dto.ORDEN_VISUALIZACION,
                    IsActive = dto.ESTADO_CATEGORIA,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PreventionCategories.Add(category);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, MapCategoryToResponseDto(category));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear la categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Update a prevention category
        /// </summary>
        [HttpPut("category/{id}")]
        public async Task<ActionResult<PreventionCategoryResponseDto>> UpdateCategory(int id, [FromBody] UpdatePreventionCategoryDto dto)
        {
            try
            {
                var category = await _context.PreventionCategories
                    .Include(c => c.Images)
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                if (!string.IsNullOrEmpty(dto.NOMBRE_CATEGORIA))
                    category.Name = dto.NOMBRE_CATEGORIA;
                if (dto.DESCRIPCION_CATEGORIA != null)
                    category.Description = dto.DESCRIPCION_CATEGORIA;
                if (dto.ICONO != null)
                    category.Icon = dto.ICONO;
                if (dto.COLOR != null)
                    category.Color = dto.COLOR;
                if (dto.ORDEN_VISUALIZACION.HasValue)
                    category.DisplayOrder = dto.ORDEN_VISUALIZACION.Value;
                if (dto.ESTADO_CATEGORIA.HasValue)
                    category.IsActive = dto.ESTADO_CATEGORIA.Value;

                await _context.SaveChangesAsync();

                return Ok(MapCategoryToResponseDto(category));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a prevention category (and all related images/items)
        /// </summary>
        [HttpDelete("category/{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.PreventionCategories
                    .Include(c => c.Images)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                // Delete images from MongoDB
                foreach (var image in category.Images)
                {
                    try
                    {
                        _mongoConnection.DeleteImage(image.MongoImageId);
                    }
                    catch { /* Ignore MongoDB errors */ }
                }

                _context.PreventionCategories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Categoría eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Reorder categories
        /// </summary>
        [HttpPut("categories/reorder")]
        public async Task<ActionResult> ReorderCategories([FromBody] ReorderRequestDto dto)
        {
            try
            {
                foreach (var item in dto.Items)
                {
                    var category = await _context.PreventionCategories.FindAsync(item.Id);
                    if (category != null)
                    {
                        category.DisplayOrder = item.NewOrder;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Categorías reordenadas correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al reordenar las categorías", error = ex.Message });
            }
        }

        // ==================== IMAGES ====================

        /// <summary>
        /// Add image to a category
        /// </summary>
        [HttpPost("category/{categoryId}/image")]
        public async Task<ActionResult<PreventionImageResponseDto>> AddImage(int categoryId, [FromForm] AddPreventionImageDto dto)
        {
            try
            {
                var category = await _context.PreventionCategories.FindAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                if (dto.Imagen == null || dto.Imagen.Length == 0)
                {
                    return BadRequest(new { message = "La imagen es requerida" });
                }

                // Convert image to base64 and save to MongoDB
                string base64Image;
                using (var ms = new MemoryStream())
                {
                    await dto.Imagen.CopyToAsync(ms);
                    var imageBytes = ms.ToArray();
                    base64Image = Convert.ToBase64String(imageBytes);
                }

                var mongoImage = new ImagenModel
                {
                    Imagen = base64Image,
                    Name = dto.Imagen.FileName
                };

                var mongoId = _mongoConnection.UploadImage(mongoImage);

                // Get max display order
                var maxOrder = await _context.PreventionCategoryImages
                    .Where(i => i.CategoryId == categoryId)
                    .MaxAsync(i => (int?)i.DisplayOrder) ?? 0;

                var categoryImage = new PreventionCategoryImage
                {
                    CategoryId = categoryId,
                    MongoImageId = mongoId,
                    Title = dto.TITULO_IMAGEN,
                    DisplayOrder = dto.ORDEN_VISUALIZACION > 0 ? dto.ORDEN_VISUALIZACION : maxOrder + 1,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PreventionCategoryImages.Add(categoryImage);
                await _context.SaveChangesAsync();

                return Ok(new PreventionImageResponseDto
                {
                    ID_IMAGEN_CATEGORIA = categoryImage.Id,
                    ID_IMAGEN_MONGO = categoryImage.MongoImageId,
                    TITULO_IMAGEN = categoryImage.Title,
                    ORDEN_VISUALIZACION = categoryImage.DisplayOrder
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al agregar la imagen", error = ex.Message });
            }
        }

        /// <summary>
        /// Update image metadata
        /// </summary>
        [HttpPut("image/{id}")]
        public async Task<ActionResult<PreventionImageResponseDto>> UpdateImage(int id, [FromBody] UpdatePreventionImageDto dto)
        {
            try
            {
                var image = await _context.PreventionCategoryImages.FindAsync(id);
                if (image == null)
                {
                    return NotFound(new { message = "Imagen no encontrada" });
                }

                if (dto.TITULO_IMAGEN != null)
                    image.Title = dto.TITULO_IMAGEN;
                if (dto.ORDEN_VISUALIZACION.HasValue)
                    image.DisplayOrder = dto.ORDEN_VISUALIZACION.Value;

                await _context.SaveChangesAsync();

                return Ok(new PreventionImageResponseDto
                {
                    ID_IMAGEN_CATEGORIA = image.Id,
                    ID_IMAGEN_MONGO = image.MongoImageId,
                    TITULO_IMAGEN = image.Title,
                    ORDEN_VISUALIZACION = image.DisplayOrder
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la imagen", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an image
        /// </summary>
        [HttpDelete("image/{id}")]
        public async Task<ActionResult> DeleteImage(int id)
        {
            try
            {
                var image = await _context.PreventionCategoryImages.FindAsync(id);
                if (image == null)
                {
                    return NotFound(new { message = "Imagen no encontrada" });
                }

                // Delete from MongoDB
                try
                {
                    _mongoConnection.DeleteImage(image.MongoImageId);
                }
                catch { /* Ignore MongoDB errors */ }

                _context.PreventionCategoryImages.Remove(image);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Imagen eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la imagen", error = ex.Message });
            }
        }

        /// <summary>
        /// Reorder images within a category
        /// </summary>
        [HttpPut("images/reorder")]
        public async Task<ActionResult> ReorderImages([FromBody] ReorderRequestDto dto)
        {
            try
            {
                foreach (var item in dto.Items)
                {
                    var image = await _context.PreventionCategoryImages.FindAsync(item.Id);
                    if (image != null)
                    {
                        image.DisplayOrder = item.NewOrder;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Imágenes reordenadas correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al reordenar las imágenes", error = ex.Message });
            }
        }

        // ==================== ITEMS ====================

        /// <summary>
        /// Get items of a category
        /// </summary>
        [HttpGet("items/{categoryId}")]
        public async Task<ActionResult<List<PreventionItemResponseDto>>> GetItems(int categoryId)
        {
            try
            {
                var items = await _context.PreventionItems
                    .Where(i => i.CategoryId == categoryId)
                    .Include(i => i.Images.OrderBy(img => img.DisplayOrder))
                    .OrderBy(i => i.DisplayOrder)
                    .ToListAsync();

                var response = items.Select(MapItemToResponseDto).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los items", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new prevention item
        /// </summary>
        [HttpPost("item")]
        public async Task<ActionResult<PreventionItemResponseDto>> CreateItem([FromBody] CreatePreventionItemDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var category = await _context.PreventionCategories.FindAsync(dto.FK_ID_CATEGORIA_PREVENCION);
                if (category == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                // Get max display order
                var maxOrder = await _context.PreventionItems
                    .Where(i => i.CategoryId == dto.FK_ID_CATEGORIA_PREVENCION)
                    .MaxAsync(i => (int?)i.DisplayOrder) ?? 0;

                var item = new PreventionItem
                {
                    CategoryId = dto.FK_ID_CATEGORIA_PREVENCION,
                    Title = dto.TITULO_ITEM,
                    Description = dto.DESCRIPCION_ITEM,
                    Emoji = dto.EMOJI_ITEM,
                    IsWarning = dto.ES_ADVERTENCIA,
                    DisplayOrder = dto.ORDEN_VISUALIZACION > 0 ? dto.ORDEN_VISUALIZACION : maxOrder + 1,
                    IsActive = dto.ESTADO_ITEM,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PreventionItems.Add(item);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetItems), new { categoryId = item.CategoryId }, MapItemToResponseDto(item));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el item", error = ex.Message });
            }
        }

        /// <summary>
        /// Update a prevention item
        /// </summary>
        [HttpPut("item/{id}")]
        public async Task<ActionResult<PreventionItemResponseDto>> UpdateItem(int id, [FromBody] UpdatePreventionItemDto dto)
        {
            try
            {
                var item = await _context.PreventionItems.FindAsync(id);
                if (item == null)
                {
                    return NotFound(new { message = "Item no encontrado" });
                }

                if (!string.IsNullOrEmpty(dto.TITULO_ITEM))
                    item.Title = dto.TITULO_ITEM;
                if (!string.IsNullOrEmpty(dto.DESCRIPCION_ITEM))
                    item.Description = dto.DESCRIPCION_ITEM;
                if (dto.EMOJI_ITEM != null)
                    item.Emoji = dto.EMOJI_ITEM;
                if (dto.ES_ADVERTENCIA.HasValue)
                    item.IsWarning = dto.ES_ADVERTENCIA.Value;
                if (dto.ORDEN_VISUALIZACION.HasValue)
                    item.DisplayOrder = dto.ORDEN_VISUALIZACION.Value;
                if (dto.ESTADO_ITEM.HasValue)
                    item.IsActive = dto.ESTADO_ITEM.Value;

                await _context.SaveChangesAsync();

                return Ok(MapItemToResponseDto(item));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el item", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a prevention item
        /// </summary>
        [HttpDelete("item/{id}")]
        public async Task<ActionResult> DeleteItem(int id)
        {
            try
            {
                var item = await _context.PreventionItems
                    .Include(i => i.Images)
                    .FirstOrDefaultAsync(i => i.Id == id);
                if (item == null)
                {
                    return NotFound(new { message = "Item no encontrado" });
                }

                // Delete images from MongoDB
                foreach (var image in item.Images)
                {
                    try
                    {
                        _mongoConnection.DeleteImage(image.MongoImageId);
                    }
                    catch { /* Ignore MongoDB errors */ }
                }

                _context.PreventionItems.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Item eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el item", error = ex.Message });
            }
        }

        /// <summary>
        /// Reorder items within a category
        /// </summary>
        [HttpPut("items/reorder")]
        public async Task<ActionResult> ReorderItems([FromBody] ReorderRequestDto dto)
        {
            try
            {
                foreach (var reorderItem in dto.Items)
                {
                    var item = await _context.PreventionItems.FindAsync(reorderItem.Id);
                    if (item != null)
                    {
                        item.DisplayOrder = reorderItem.NewOrder;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Items reordenados correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al reordenar los items", error = ex.Message });
            }
        }

        // ==================== ITEM IMAGES ====================

        /// <summary>
        /// Add image to an item
        /// </summary>
        [HttpPost("item/{itemId}/image")]
        public async Task<ActionResult<PreventionItemImageResponseDto>> AddItemImage(int itemId, [FromForm] AddPreventionItemImageDto dto)
        {
            try
            {
                var item = await _context.PreventionItems.FindAsync(itemId);
                if (item == null)
                {
                    return NotFound(new { message = "Item no encontrado" });
                }

                if (dto.Imagen == null || dto.Imagen.Length == 0)
                {
                    return BadRequest(new { message = "La imagen es requerida" });
                }

                // Convert image to base64 and save to MongoDB
                string base64Image;
                using (var ms = new MemoryStream())
                {
                    await dto.Imagen.CopyToAsync(ms);
                    var imageBytes = ms.ToArray();
                    base64Image = Convert.ToBase64String(imageBytes);
                }

                var mongoImage = new ImagenModel
                {
                    Imagen = base64Image,
                    Name = dto.Imagen.FileName
                };

                var mongoId = _mongoConnection.UploadImage(mongoImage);

                // Get max display order
                var maxOrder = await _context.PreventionItemImages
                    .Where(i => i.ItemId == itemId)
                    .MaxAsync(i => (int?)i.DisplayOrder) ?? 0;

                var itemImage = new PreventionItemImage
                {
                    ItemId = itemId,
                    MongoImageId = mongoId,
                    Title = dto.TITULO_IMAGEN,
                    DisplayOrder = dto.ORDEN_VISUALIZACION > 0 ? dto.ORDEN_VISUALIZACION : maxOrder + 1,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PreventionItemImages.Add(itemImage);
                await _context.SaveChangesAsync();

                return Ok(new PreventionItemImageResponseDto
                {
                    ID_IMAGEN_ITEM = itemImage.Id,
                    ID_IMAGEN_MONGO = itemImage.MongoImageId,
                    TITULO_IMAGEN = itemImage.Title,
                    ORDEN_VISUALIZACION = itemImage.DisplayOrder
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al agregar la imagen al item", error = ex.Message });
            }
        }

        /// <summary>
        /// Update item image metadata
        /// </summary>
        [HttpPut("item-image/{id}")]
        public async Task<ActionResult<PreventionItemImageResponseDto>> UpdateItemImage(int id, [FromBody] UpdatePreventionImageDto dto)
        {
            try
            {
                var image = await _context.PreventionItemImages.FindAsync(id);
                if (image == null)
                {
                    return NotFound(new { message = "Imagen no encontrada" });
                }

                if (dto.TITULO_IMAGEN != null)
                    image.Title = dto.TITULO_IMAGEN;
                if (dto.ORDEN_VISUALIZACION.HasValue)
                    image.DisplayOrder = dto.ORDEN_VISUALIZACION.Value;

                await _context.SaveChangesAsync();

                return Ok(new PreventionItemImageResponseDto
                {
                    ID_IMAGEN_ITEM = image.Id,
                    ID_IMAGEN_MONGO = image.MongoImageId,
                    TITULO_IMAGEN = image.Title,
                    ORDEN_VISUALIZACION = image.DisplayOrder
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la imagen", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an item image
        /// </summary>
        [HttpDelete("item-image/{id}")]
        public async Task<ActionResult> DeleteItemImage(int id)
        {
            try
            {
                var image = await _context.PreventionItemImages.FindAsync(id);
                if (image == null)
                {
                    return NotFound(new { message = "Imagen no encontrada" });
                }

                // Delete from MongoDB
                try
                {
                    _mongoConnection.DeleteImage(image.MongoImageId);
                }
                catch { /* Ignore MongoDB errors */ }

                _context.PreventionItemImages.Remove(image);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Imagen eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la imagen", error = ex.Message });
            }
        }

        /// <summary>
        /// Reorder images within an item
        /// </summary>
        [HttpPut("item-images/reorder")]
        public async Task<ActionResult> ReorderItemImages([FromBody] ReorderRequestDto dto)
        {
            try
            {
                foreach (var reorderItem in dto.Items)
                {
                    var image = await _context.PreventionItemImages.FindAsync(reorderItem.Id);
                    if (image != null)
                    {
                        image.DisplayOrder = reorderItem.NewOrder;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Imágenes reordenadas correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al reordenar las imágenes", error = ex.Message });
            }
        }

        // ==================== HELPER METHODS ====================

        private static PreventionCategoryResponseDto MapCategoryToResponseDto(PreventionCategory category)
        {
            return new PreventionCategoryResponseDto
            {
                ID_CATEGORIA_PREVENCION = category.Id,
                NOMBRE_CATEGORIA = category.Name,
                DESCRIPCION_CATEGORIA = category.Description,
                ICONO = category.Icon,
                COLOR = category.Color,
                ORDEN_VISUALIZACION = category.DisplayOrder,
                ESTADO_CATEGORIA = category.IsActive,
                FECHA_CREACION = category.CreatedAt,
                IMAGENES = category.Images?.Select(i => new PreventionImageResponseDto
                {
                    ID_IMAGEN_CATEGORIA = i.Id,
                    ID_IMAGEN_MONGO = i.MongoImageId,
                    TITULO_IMAGEN = i.Title,
                    ORDEN_VISUALIZACION = i.DisplayOrder
                }).ToList() ?? new List<PreventionImageResponseDto>(),
                ITEMS = category.Items?.Select(MapItemToResponseDto).ToList() ?? new List<PreventionItemResponseDto>()
            };
        }

        private static PreventionItemResponseDto MapItemToResponseDto(PreventionItem item)
        {
            return new PreventionItemResponseDto
            {
                ID_ITEM_PREVENCION = item.Id,
                FK_ID_CATEGORIA_PREVENCION = item.CategoryId,
                TITULO_ITEM = item.Title,
                DESCRIPCION_ITEM = item.Description,
                EMOJI_ITEM = item.Emoji,
                ES_ADVERTENCIA = item.IsWarning,
                ORDEN_VISUALIZACION = item.DisplayOrder,
                ESTADO_ITEM = item.IsActive,
                IMAGENES = item.Images?.Select(img => new PreventionItemImageResponseDto
                {
                    ID_IMAGEN_ITEM = img.Id,
                    ID_IMAGEN_MONGO = img.MongoImageId,
                    TITULO_IMAGEN = img.Title,
                    ORDEN_VISUALIZACION = img.DisplayOrder
                }).ToList() ?? new List<PreventionItemImageResponseDto>()
            };
        }
    }
}
