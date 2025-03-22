namespace Backend_App_Dengue.Model
{
    public class HospitalModel
    {
        public int ID_HOSPITAL { get; set; }
        public string NOMBRE_HOSPITAL { get; set; }
        public int ESTADO_HOSPITAL { get; set; }
        public string DIRECCION_HOSPITAL { get; set; }
        public string LATITUD_HOSPITAL { get; set; }
        public string LONGITUD_HOSPITAL { get; set; }
        public int FK_ID_MUNICIPIO { get; set; }
        public string IMAGEN_HOSPITAL { get; set; }
        //public IFormFile? File { get; set; }
        //public ImagenModel Imagen { get; set; }
        public int CANTIDADCASOS_HOSPITAL { get; set; }
        public int ID_DEPARTAMENTO { get; set; }
    }
}
