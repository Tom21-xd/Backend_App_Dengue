namespace Backend_App_Dengue.Model
{
    public class CaseModel
    {
        public int ID_CASOREPORTADO { get; set; }
        public string DESCRIPCION_CASOREPORTADO { get; set; }
        public string FECHA_CASOREPORTADO { get; set; }
        public int FK_ID_ESTADOCASO { get; set; }
        public string NOMBRE_ESTADOCASO { get; set; }
        public int ID_DEPARTAMENTO { get; set; }
        public int ID_MUNICIPIO { get; set; }
        public int FK_ID_HOSPITAL { get; set; }
        public int FK_ID_TIPODENGUE { get; set; }
        public int FK_ID_PACIENTE { get; set; }
        public string NOMBRE_PACIENTE { get; set; }
        public int FK_ID_PERSONALMEDICO { get; set; }
        public string NOMBRE_PERSONALMEDICO { get; set; }
        public string FECHAFINALIZACION_CASO { get; set; }
        public string DIRECCION_CASOREPORTADO { get; set; }
    }
}
