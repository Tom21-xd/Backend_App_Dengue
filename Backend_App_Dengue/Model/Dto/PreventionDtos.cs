using System.ComponentModel.DataAnnotations;

namespace Backend_App_Dengue.Model.Dto
{
    // ==================== Response DTOs ====================

    public class PreventionCategoryResponseDto
    {
        public int ID_CATEGORIA_PREVENCION { get; set; }
        public string NOMBRE_CATEGORIA { get; set; } = string.Empty;
        public string? DESCRIPCION_CATEGORIA { get; set; }
        public string? ICONO { get; set; }
        public string? COLOR { get; set; }
        public int ORDEN_VISUALIZACION { get; set; }
        public bool ESTADO_CATEGORIA { get; set; }
        public DateTime FECHA_CREACION { get; set; }
        public List<PreventionImageResponseDto> IMAGENES { get; set; } = new();
        public List<PreventionItemResponseDto> ITEMS { get; set; } = new();
    }

    public class PreventionCategoryListDto
    {
        public int ID_CATEGORIA_PREVENCION { get; set; }
        public string NOMBRE_CATEGORIA { get; set; } = string.Empty;
        public string? DESCRIPCION_CATEGORIA { get; set; }
        public string? ICONO { get; set; }
        public string? COLOR { get; set; }
        public int ORDEN_VISUALIZACION { get; set; }
        public bool ESTADO_CATEGORIA { get; set; }
        public int TOTAL_IMAGENES { get; set; }
        public int TOTAL_ITEMS { get; set; }
    }

    public class PreventionImageResponseDto
    {
        public int ID_IMAGEN_CATEGORIA { get; set; }
        public string ID_IMAGEN_MONGO { get; set; } = string.Empty;
        public string? TITULO_IMAGEN { get; set; }
        public int ORDEN_VISUALIZACION { get; set; }
    }

    public class PreventionItemResponseDto
    {
        public int ID_ITEM_PREVENCION { get; set; }
        public int FK_ID_CATEGORIA_PREVENCION { get; set; }
        public string TITULO_ITEM { get; set; } = string.Empty;
        public string DESCRIPCION_ITEM { get; set; } = string.Empty;
        public string? EMOJI_ITEM { get; set; }
        public bool ES_ADVERTENCIA { get; set; }
        public int ORDEN_VISUALIZACION { get; set; }
        public bool ESTADO_ITEM { get; set; }
    }

    // ==================== Create DTOs ====================

    public class CreatePreventionCategoryDto
    {
        [Required(ErrorMessage = "El nombre de la categoría es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string NOMBRE_CATEGORIA { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? DESCRIPCION_CATEGORIA { get; set; }

        [MaxLength(50)]
        public string? ICONO { get; set; }

        [MaxLength(20)]
        public string? COLOR { get; set; }

        public int ORDEN_VISUALIZACION { get; set; }

        public bool ESTADO_CATEGORIA { get; set; } = true;
    }

    public class CreatePreventionItemDto
    {
        [Required(ErrorMessage = "La categoría es requerida")]
        public int FK_ID_CATEGORIA_PREVENCION { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [MaxLength(150, ErrorMessage = "El título no puede exceder 150 caracteres")]
        public string TITULO_ITEM { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string DESCRIPCION_ITEM { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? EMOJI_ITEM { get; set; }

        public bool ES_ADVERTENCIA { get; set; } = false;

        public int ORDEN_VISUALIZACION { get; set; }

        public bool ESTADO_ITEM { get; set; } = true;
    }

    public class AddPreventionImageDto
    {
        [Required(ErrorMessage = "La categoría es requerida")]
        public int FK_ID_CATEGORIA_PREVENCION { get; set; }

        [Required(ErrorMessage = "La imagen es requerida")]
        public IFormFile Imagen { get; set; } = null!;

        [MaxLength(100)]
        public string? TITULO_IMAGEN { get; set; }

        public int ORDEN_VISUALIZACION { get; set; }
    }

    // ==================== Update DTOs ====================

    public class UpdatePreventionCategoryDto
    {
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string? NOMBRE_CATEGORIA { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? DESCRIPCION_CATEGORIA { get; set; }

        [MaxLength(50)]
        public string? ICONO { get; set; }

        [MaxLength(20)]
        public string? COLOR { get; set; }

        public int? ORDEN_VISUALIZACION { get; set; }

        public bool? ESTADO_CATEGORIA { get; set; }
    }

    public class UpdatePreventionItemDto
    {
        [MaxLength(150, ErrorMessage = "El título no puede exceder 150 caracteres")]
        public string? TITULO_ITEM { get; set; }

        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? DESCRIPCION_ITEM { get; set; }

        [MaxLength(10)]
        public string? EMOJI_ITEM { get; set; }

        public bool? ES_ADVERTENCIA { get; set; }

        public int? ORDEN_VISUALIZACION { get; set; }

        public bool? ESTADO_ITEM { get; set; }
    }

    public class UpdatePreventionImageDto
    {
        [MaxLength(100)]
        public string? TITULO_IMAGEN { get; set; }

        public int? ORDEN_VISUALIZACION { get; set; }
    }

    // ==================== Reorder DTOs ====================

    public class ReorderItemDto
    {
        public int Id { get; set; }
        public int NewOrder { get; set; }
    }

    public class ReorderRequestDto
    {
        [Required]
        public List<ReorderItemDto> Items { get; set; } = new();
    }
}
