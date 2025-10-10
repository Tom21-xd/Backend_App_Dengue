namespace Backend_App_Dengue.Model.Dto
{
    public class CreatePublicationModelDto
    {

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public IFormFile imagen { get; set; }

        public string UsuarioId { get; set; }
    }
}
