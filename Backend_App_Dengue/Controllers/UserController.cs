using Backend_App_Dengue.Data;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend_App_Dengue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        internal Connection cn = new Connection();

        [HttpGet]
        [Route("getUsers")]
        public IActionResult getUsers()
        {
            try
            {
                DataTable usu = cn.ProcedimientosSelect(null, "ListarUsuarios", null);
                List<UserModel> usuarios = usu.DataTableToList<UserModel>();

                if (usuarios == null)
                {
                    return Ok(new List<UserModel>());
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("getUser")]
        public IActionResult getUser([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "El ID del usuario es requerido" });
            }

            try
            {
                string[] datos = { id };
                string[] parametros = { "idu" };
                DataTable usu = cn.ProcedimientosSelect(parametros, "ObtenerUsuario", datos);
                List<UserModel> usuarios = usu.DataTableToList<UserModel>();

                if (usuarios == null || usuarios.Count == 0)
                {
                    return NotFound(new { message = "No se ha encontrado el usuario" });
                }

                return Ok(usuarios[0]);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el usuario", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("getUserLive")]
        public IActionResult getUserLive()
        {
            try
            {
                DataTable usu = cn.ProcedimientosSelect(null, "ListarUsuarioSanos", null);
                List<UserModel> usuarios = usu.DataTableToList<UserModel>();

                if (usuarios == null)
                {
                    return Ok(new List<UserModel>());
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios sanos", error = ex.Message });
            }
        }

        // HU-004: Actualizar Perfil Propio
        [HttpPut]
        [Route("updateProfile/{id}")]
        public IActionResult UpdateProfile(int id, [FromBody] UpdateUserDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos del usuario son requeridos" });
            }

            try
            {
                string[] parametros = { "idu", "nombre", "correo", "dire", "rolu", "muni", "gene" };
                string[] valores = {
                    id.ToString(),
                    dto.Nombre ?? "",
                    dto.Correo ?? "",
                    dto.Direccion ?? "",
                    dto.IdRol?.ToString() ?? "1",
                    dto.IdMunicipio?.ToString() ?? "1",
                    dto.IdGenero?.ToString() ?? "1"
                };

                cn.procedimientosInEd(parametros, "EditarUsuario", valores);

                return Ok(new { message = "Perfil actualizado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el perfil", error = ex.Message });
            }
        }

        // HU-005: Actualizar Usuario (Admin)
        [HttpPut]
        [Route("updateUser/{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Los datos del usuario son requeridos" });
            }

            try
            {
                string[] parametros = { "idu", "nombre", "correo", "dire", "rolu", "muni", "gene" };
                string[] valores = {
                    id.ToString(),
                    dto.Nombre ?? "",
                    dto.Correo ?? "",
                    dto.Direccion ?? "",
                    dto.IdRol?.ToString() ?? "1",
                    dto.IdMunicipio?.ToString() ?? "1",
                    dto.IdGenero?.ToString() ?? "1"
                };

                cn.procedimientosInEd(parametros, "EditarUsuario", valores);

                return Ok(new { message = "Usuario actualizado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el usuario", error = ex.Message });
            }
        }

        // HU-005: Eliminar Usuario (Admin)
        [HttpDelete]
        [Route("deleteUser/{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                string[] parametros = { "idu" };
                string[] valores = { id.ToString() };

                cn.procedimientosInEd(parametros, "EliminarUsuario", valores);

                return Ok(new { message = "Usuario eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el usuario", error = ex.Message });
            }
        }

        // HU-005: Buscar Usuarios
        [HttpGet]
        [Route("searchUsers")]
        public IActionResult SearchUsers([FromQuery] string? filter, [FromQuery] int? roleId)
        {
            try
            {
                string[] parametros = { "filtro", "id_rol" };
                string[] valores = {
                    filter ?? "",
                    roleId?.ToString() ?? "NULL"
                };

                DataTable usu = cn.ProcedimientosSelect(parametros, "BuscarUsuarios", valores);
                List<UserModel> usuarios = usu.DataTableToList<UserModel>();

                if (usuarios == null)
                {
                    return Ok(new List<UserModel>());
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar usuarios", error = ex.Message });
            }
        }
    }
}
