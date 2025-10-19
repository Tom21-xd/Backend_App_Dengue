using System.ComponentModel.DataAnnotations;

namespace Backend_App_Dengue.Model.Dto
{
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "El contenido del comentario es requerido")]
        [MaxLength(1000, ErrorMessage = "El comentario no puede exceder 1000 caracteres")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// ID del comentario padre si es una respuesta
        /// </summary>
        public int? ParentCommentId { get; set; }
    }
}
