namespace Backend_App_Dengue.Model
{
    public class CreatePublicationModel
    {

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public IFormFile imagen { get; set; }

        public string UsuarioId { get; set; }
    }
}
