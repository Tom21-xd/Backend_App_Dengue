using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class RoleModel
    {
        [JsonPropertyName("ID_ROL")]
        public int ID_ROL { get; set; }
        [JsonPropertyName("NOMBRE_ROL")]
        public string NOMBRE_ROL { get; set; }
        [JsonPropertyName("ESTADO_ROL")]
        public int ESTADO_ROL { get; set; }
    }
}
