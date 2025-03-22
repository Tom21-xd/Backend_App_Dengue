namespace Backend_App_Dengue.Model
{
    public class UserModel
    {
        public int ID_USUARIO { get; set; }
        public string NOMBRE_USUARIO { get; set; }
        public string CORREO_USUARIO { get; set; }
        public string CONTRASENIA_USUARIO { get; set; }
        public string DIRECCION_USUARIO { get; set; }
        public int FK_ID_ROL { get; set; }
        public string NOMBRE_ROL { get; set; }
        public int FK_ID_MUNICIPIO { get; set; }
        public string NOMBRE_MUNICIPIO { get; set; }
        public int FK_ID_TIPOSANGRE { get; set; }
        public string NOMBRE_TIPOSANGRE { get; set; }
        public int FK_ID_GENERO { get; set; }
        public string NOMBRE_GENERO { get; set; }
        public int ID_DEPARTAMENTO { get; set; }
        public string NOMBRE_ESTADOUSUARIO { get; set; }
    }
}
