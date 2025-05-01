using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class PublicationModel
    {
        [JsonPropertyName("ID_PUBLICACION")]
        public int ID_PUBLICACION { get; set; }
        [JsonPropertyName("TITULO_PUBLICACION")]
        public string TITULO_PUBLICACION { get; set; }
        [JsonPropertyName("IMAGEN_PUBLICACION")]
        public string IMAGEN_PUBLICACION { get; set; }
        [JsonPropertyName("DESCRIPCION_PUBLICACION")]
        public string DESCRIPCION_PUBLICACION { get; set; }
        [JsonPropertyName("FECHA_PUBLICACION")]
        public string FECHA_PUBLICACION { get; set; }
        [JsonPropertyName("FK_ID_USUARIO")]
        public int FK_ID_USUARIO { get; set; }
        [JsonPropertyName("NOMBRE_USUARIO")]
        public string NOMBRE_USUARIO { get; set; }

        //public IFormFile? File { get; set; }
        //public ImagenModel Imagen { get; set; }
    }
}
