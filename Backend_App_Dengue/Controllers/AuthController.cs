using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Auth;
using Backend_App_Dengue.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        internal Connection cn = new Connection();

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> login([FromBody] LoginModel user)
        {
            string[] datos = { user.email, user.password };
            string[] parametros = { "correo", "contra" };

            DataTable usu = cn.ProcedimientosSelect(parametros, "ValidarUsuario", datos);
            List<UserModel> usuarios = usu.DataTableToList<UserModel>();

            if (usuarios.Count == 0)
            {
                return Unauthorized(new { message = "No se ha encontrado al usuario" });
            }

            var usuario = usuarios[0];

            return Ok(new { message = "Usuario autenticado con éxito", usuario = usuario });
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> register([FromBody] RegisterUserModel usuario)
        {
            string[] parametros = { "nomu", "correou","contra", "diru", "rolu", "muniu", "tiposangreu", "genu" };
            string[] valores = { usuario.NOMBRE_USUARIO, usuario.CORREO_USUARIO,usuario.CONTRASENIA_USUARIO, usuario.DIRECCION_USUARIO, usuario.FK_ID_ROL + "", usuario.FK_ID_MUNICIPIO.ToString(), usuario.FK_ID_TIPOSANGRE.ToString(), usuario.FK_ID_GENERO + "" };
            cn.procedimientosInEd(parametros, "RegistrarUsuario", valores);
            return Ok(new { message = "Usuario creado con éxito" });
        }

        [HttpPost]
        [Route("recoverPassword")]
        public async Task<IActionResult> recoverPassword([FromBody] string email)
        {
            ServiceGmail aux = new ServiceGmail();
            string newPassword = generarclave();
            string[] datos = { email, newPassword };
            string[] parametros = { "correo", "contra" };
            cn.procedimientosInEd(parametros, "RecuperarContra", datos);
            aux.SendEmailGmail(email, "Recuperacion De Contraseña", "Su nueva Contraseña es :" + newPassword);
            return Ok(new { message = "Contraseña Enviada al correo con exito" });
        }

        private String generarclave()
        {
            Random aleatorio = new Random();
            string conjuntoCaracteres = "abcdefghijklmnopqrstuvwxyz0123456789";
            string cadena = "";
            for (int i = 0; i < 6; i++)
            {
                cadena += conjuntoCaracteres[aleatorio.Next(conjuntoCaracteres.Length)];
            }
            return cadena;
        }
    }
}
