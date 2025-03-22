namespace Backend_App_Dengue.Model
{
    public class CityModel
    {
        public int ID_MUNICIPIO { get; set; }
        public string NOMBRE_MUNICIPIO { get; set; }
        public int FK_ID_DEPARTAMENTO { get; set; }
    }
}
