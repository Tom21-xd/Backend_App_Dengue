using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Backend_App_Dengue.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        internal Connection cn = new Connection();

        [HttpPost]
        [Route("login")]
        public IActionResult login([FromBody] LoginModelDto user)
        {
            if (user == null || string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.password))
            {
                return BadRequest(new { message = "Email y contraseña son requeridos" });
            }

            try
            {
                string hashedPassword = HashPassword(user.password);
                string[] datos = { user.email, hashedPassword };
                string[] parametros = { "correo", "contra" };

                DataTable usu = cn.ProcedimientosSelect(parametros, "ValidarUsuario", datos);
                List<UserModel> usuarios = usu.DataTableToList<UserModel>();

                if (usuarios == null || usuarios.Count == 0)
                {
                    return Unauthorized(new { message = "No se ha encontrado al usuario" });
                }

                var usuario = usuarios[0];

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al iniciar sesión", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("register")]
        public IActionResult register([FromBody] RegisterUserModel usuario)
        {
            if (usuario == null)
            {
                return BadRequest(new { message = "Los datos del usuario son requeridos" });
            }

            try
            {
                string hashedPassword = HashPassword(usuario.CONTRASENIA_USUARIO);
                string[] parametros = { "nomu", "correou", "contra", "diru", "rolu", "muniu", "tiposangreu", "genu" };
                string[] valores = {
                    usuario.NOMBRE_USUARIO,
                    usuario.CORREO_USUARIO,
                    hashedPassword,
                    usuario.DIRECCION_USUARIO,
                    usuario.FK_ID_ROL.ToString(),
                    usuario.FK_ID_MUNICIPIO.ToString(),
                    usuario.FK_ID_TIPOSANGRE.ToString(),
                    usuario.FK_ID_GENERO.ToString()
                };
                cn.procedimientosInEd(parametros, "RegistrarUsuario", valores);
                return Ok(new { message = "Usuario creado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al registrar usuario", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("recoverPassword")]
        public async Task<IActionResult> recoverPassword([FromBody] RecoverPasswordDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "El email es requerido" });
            }

            try
            {
                ServiceGmail aux = new ServiceGmail();
                string newPassword = GenerarClaveSegura();
                string hashedPassword = HashPassword(newPassword);
                string[] datos = { request.Email, hashedPassword };
                string[] parametros = { "correo", "contra" };
                cn.procedimientosInEd(parametros, "RecuperarContra", datos);
                await Task.Run(() => aux.SendEmailGmail(request.Email, "Recuperación de Contraseña",
                    $"Su nueva contraseña es: <strong>{newPassword}</strong><br><br>Por favor, cámbiela después de iniciar sesión."));
                return Ok(new { message = "Contraseña enviada al correo con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al recuperar contraseña", error = ex.Message });
            }
        }

        private string GenerarClaveSegura()
        {
            const string caracteresMinusculas = "abcdefghijklmnopqrstuvwxyz";
            const string caracteresMayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string caracteresNumeros = "0123456789";
            const string caracteresEspeciales = "!@#$%&*";
            const string todosCaracteres = caracteresMinusculas + caracteresMayusculas + caracteresNumeros + caracteresEspeciales;

            StringBuilder password = new StringBuilder();

            // Asegurar al menos un carácter de cada tipo
            password.Append(caracteresMinusculas[RandomNumberGenerator.GetInt32(caracteresMinusculas.Length)]);
            password.Append(caracteresMayusculas[RandomNumberGenerator.GetInt32(caracteresMayusculas.Length)]);
            password.Append(caracteresNumeros[RandomNumberGenerator.GetInt32(caracteresNumeros.Length)]);
            password.Append(caracteresEspeciales[RandomNumberGenerator.GetInt32(caracteresEspeciales.Length)]);

            // Completar hasta 12 caracteres
            for (int i = 4; i < 12; i++)
            {
                password.Append(todosCaracteres[RandomNumberGenerator.GetInt32(todosCaracteres.Length)]);
            }

            // Mezclar los caracteres
            return new string(password.ToString().OrderBy(x => RandomNumberGenerator.GetInt32(1000)).ToArray());
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [HttpPost]
        [Route("Rethus")]
        public async Task<IActionResult> Rethus([FromBody] RethusRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.PrimerNombre) ||
                string.IsNullOrEmpty(request.PrimerApellido) ||
                string.IsNullOrEmpty(request.TipoIdentificacion) ||
                string.IsNullOrEmpty(request.Cedula))
            {
                return BadRequest(new { message = "Todos los campos son requeridos" });
            }

            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("https://dvbc8l62-8085.use.devtunnels.ms");
                client.Timeout = TimeSpan.FromSeconds(30);

                var formData = new Dictionary<string, string>
                {
                    { "tipo_identificacion", request.TipoIdentificacion },
                    { "numero_identificacion", request.Cedula },
                    { "primer_nombre", request.PrimerNombre },
                    { "primer_apellido", request.PrimerApellido }
                };

                var content = new FormUrlEncodedContent(formData);

                var response = await client.PostAsync("/validar", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return Ok(new { message = "Consulta exitosa", data = result });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { message = "Error en la consulta", detail = error });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { message = "Servicio RETHUS no disponible", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al validar con RETHUS", error = ex.Message });
            }
        }
    }
}
