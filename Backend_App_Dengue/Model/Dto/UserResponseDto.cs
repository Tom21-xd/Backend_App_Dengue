using System.Text.Json.Serialization;

namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO for User responses to match Android UserModel expectations
    /// Includes all related entity names that Android expects
    /// </summary>
    public class UserResponseDto
    {
        [JsonPropertyName("ID_USUARIO")]
        public int Id { get; set; }

        [JsonPropertyName("NOMBRE_USUARIO")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("CORREO_USUARIO")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("CONTRASENIA_USUARIO")]
        public string? Password { get; set; } = null; // Usually null for security

        [JsonPropertyName("DIRECCION_USUARIO")]
        public string? Address { get; set; }

        [JsonPropertyName("FECHA_NACIMIENTO_USUARIO")]
        public string? BirthDate { get; set; }

        [JsonPropertyName("FK_ID_ROL")]
        public int RoleId { get; set; }

        [JsonPropertyName("NOMBRE_ROL")]
        public string? RoleName { get; set; }

        [JsonPropertyName("FK_ID_MUNICIPIO")]
        public int? CityId { get; set; }

        [JsonPropertyName("NOMBRE_MUNICIPIO")]
        public string? CityName { get; set; }

        [JsonPropertyName("FK_ID_TIPOSANGRE")]
        public int BloodTypeId { get; set; }

        [JsonPropertyName("NOMBRE_TIPOSANGRE")]
        public string? BloodTypeName { get; set; }

        [JsonPropertyName("FK_ID_GENERO")]
        public int GenreId { get; set; }

        [JsonPropertyName("NOMBRE_GENERO")]
        public string? GenreName { get; set; }

        [JsonPropertyName("ID_DEPARTAMENTO")]
        public int DepartmentId { get; set; }

        [JsonPropertyName("NOMBRE_ESTADOUSUARIO")]
        public string? UserStateName { get; set; }
    }
}
