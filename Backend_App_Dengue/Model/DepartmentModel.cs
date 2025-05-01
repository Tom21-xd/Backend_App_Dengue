using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model
{
    public class DepartmentModel
    {
        [JsonPropertyName("ID_DEPARTAMENTO")]
        public int ID_DEPARTAMENTO { get; set; }
        [JsonPropertyName("NOMBRE_DEPARTAMENTO")]
        public string NOMBRE_DEPARTAMENTO { get; set; }
    }
}
