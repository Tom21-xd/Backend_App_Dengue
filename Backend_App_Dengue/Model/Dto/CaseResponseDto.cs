using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    public class CaseResponseDto
    {
        [JsonPropertyName("ID_CASO")]
        public int Id { get; set; }

        [JsonPropertyName("DESCRIPCION_CASOREPORTADO")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("FECHA_CASOREPORTADO")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("FK_ID_ESTADOCASO")]
        public int StateId { get; set; }

        [JsonPropertyName("FK_ID_HOSPITAL")]
        public int HospitalId { get; set; }

        [JsonPropertyName("FK_ID_TIPODENGUE")]
        public int TypeOfDengueId { get; set; }

        [JsonPropertyName("FK_ID_PACIENTE")]
        public int PatientId { get; set; }

        [JsonPropertyName("FK_ID_PERSONALMEDICO")]
        public int? MedicalStaffId { get; set; }

        [JsonPropertyName("FECHAFINALIZACION_CASO")]
        public DateTime? FinishedAt { get; set; }

        [JsonPropertyName("DIRECCION_CASOREPORTADO")]
        public string? Address { get; set; }

        [JsonPropertyName("ESTADO_CASO")]
        public bool IsActive { get; set; }

        // Nested entities
        [JsonPropertyName("ESTADO")]
        public CaseStateInfoDto? State { get; set; }

        [JsonPropertyName("HOSPITAL")]
        public HospitalInfoDto? Hospital { get; set; }

        [JsonPropertyName("TIPO_DENGUE")]
        public TypeOfDengueInfoDto? TypeOfDengue { get; set; }

        [JsonPropertyName("PACIENTE")]
        public UserInfoDto? Patient { get; set; }

        [JsonPropertyName("PERSONAL_MEDICO")]
        public UserInfoDto? MedicalStaff { get; set; }
    }

    public class CaseStateInfoDto
    {
        [JsonPropertyName("ID_ESTADOCASO")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_ESTADOCASO")]
        public string Name { get; set; } = string.Empty;
    }

    public class HospitalInfoDto
    {
        [JsonPropertyName("ID_HOSPITAL")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_HOSPITAL")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("DIRECCION_HOSPITAL")]
        public string? Address { get; set; }

        [JsonPropertyName("LATITUD_HOSPITAL")]
        public string? Latitude { get; set; }

        [JsonPropertyName("LONGITUD_HOSPITAL")]
        public string? Longitude { get; set; }

        [JsonPropertyName("FK_ID_MUNICIPIO")]
        public int CityId { get; set; }

        [JsonPropertyName("MUNICIPIO")]
        public CityInfoDto? City { get; set; }
    }

    public class CityInfoDto
    {
        [JsonPropertyName("ID_MUNICIPIO")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_MUNICIPIO")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("FK_ID_DEPARTAMENTO")]
        public int DepartmentId { get; set; }

        [JsonPropertyName("DEPARTAMENTO")]
        public DepartmentInfoDto? Department { get; set; }
    }

    public class DepartmentInfoDto
    {
        [JsonPropertyName("ID_DEPARTAMENTO")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_DEPARTAMENTO")]
        public string Name { get; set; } = string.Empty;
    }

    public class TypeOfDengueInfoDto
    {
        [JsonPropertyName("ID_TIPODENGUE")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_TIPODENGUE")]
        public string Name { get; set; } = string.Empty;
    }
}
