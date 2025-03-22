namespace Backend_App_Dengue.Model
{
    public class CreateCaseModel
    {
        public string descripcion { get; set; }
        public int id_hospital { get; set; }
        public int id_tipoDengue { get; set; }
        public int id_paciente { get; set; }
        public int id_personalMedico { get; set; }
        public string direccion { get; set; }
    }
}
