using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Backend_App_Dengue.Model;
using Backend_App_Dengue.Model.Dto;
using Backend_App_Dengue.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace Backend_App_Dengue.Controllers
{
    [Route("Auth")]
    [ApiController]
    public class AuthControllerEF : ControllerBase
    {
        private readonly IRepository<User> _userRepository;
        private readonly JwtService _jwtService;

        public AuthControllerEF(IRepository<User> userRepository, JwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Login with email and password using BCrypt verification
        /// </summary>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDto credentials)
        {
            if (credentials == null || string.IsNullOrEmpty(credentials.email) || string.IsNullOrEmpty(credentials.password))
            {
                return BadRequest(new { message = "Email y contraseña son requeridos" });
            }

            try
            {
                // Find user by email
                var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == credentials.email);

                if (user == null)
                {
                    return Unauthorized(new { message = "No se ha encontrado al usuario" });
                }

                // Verify password with BCrypt
                bool isValidPassword = BCrypt.Net.BCrypt.Verify(credentials.password, user.Password);

                if (!isValidPassword)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "Usuario inactivo" });
                }

                // Generate JWT token
                string token = _jwtService.GenerateToken(user);

                return Ok(new
                {
                    message = "Login exitoso",
                    token = token,
                    user = new
                    {
                        id = user.Id,
                        nombre = user.Name,
                        email = user.Email,
                        roleId = user.RoleId,
                        isActive = user.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al iniciar sesión", error = ex.Message });
            }
        }

        /// <summary>
        /// Register a new user with BCrypt password hashing
        /// </summary>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserModel usuario)
        {
            if (usuario == null)
            {
                return BadRequest(new { message = "Los datos del usuario son requeridos" });
            }

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(usuario.NOMBRE_USUARIO) ||
                    string.IsNullOrWhiteSpace(usuario.CORREO_USUARIO) ||
                    string.IsNullOrWhiteSpace(usuario.CONTRASENIA_USUARIO))
                {
                    return BadRequest(new { message = "Nombre, correo y contraseña son requeridos" });
                }

                // Check if email already exists
                var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == usuario.CORREO_USUARIO);

                if (existingUser != null)
                {
                    return Conflict(new { message = "El correo ya se encuentra registrado" });
                }

                // Hash password with BCrypt (automatically generates salt)
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuario.CONTRASENIA_USUARIO);

                // Create new user
                var newUser = new User
                {
                    Name = usuario.NOMBRE_USUARIO,
                    Email = usuario.CORREO_USUARIO,
                    Password = hashedPassword,
                    Address = usuario.DIRECCION_USUARIO ?? string.Empty,
                    RoleId = usuario.FK_ID_ROL,
                    CityId = usuario.FK_ID_MUNICIPIO,
                    BloodTypeId = usuario.FK_ID_TIPOSANGRE,
                    GenreId = usuario.FK_ID_GENERO,
                    IsActive = true
                };

                var createdUser = await _userRepository.AddAsync(newUser);

                // Return created user (password will not be exposed in response due to navigation properties)
                return Ok(new { message = "Usuario creado con éxito", usuario = createdUser });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar usuario: {ex.Message}");
                return StatusCode(500, new { message = "Error al registrar usuario. Por favor, intenta nuevamente." });
            }
        }

        /// <summary>
        /// Recover password - generates new random password and sends via email
        /// </summary>
        [HttpPost]
        [Route("recoverPassword")]
        public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "El email es requerido" });
            }

            try
            {
                // Find user by email
                var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    // Don't reveal if email exists or not (security best practice)
                    return Ok(new { message = "Si el correo existe, se enviará la nueva contraseña" });
                }

                // Generate new secure password
                string newPassword = GenerarClaveSegura();

                // Hash new password with BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                // Update user password
                user.Password = hashedPassword;
                await _userRepository.UpdateAsync(user);

                // Send email with new password
                ServiceGmail emailService = new ServiceGmail();
                await Task.Run(() => emailService.SendEmailGmail(
                    request.Email,
                    "Recuperación de Contraseña",
                    $"Su nueva contraseña es: <strong>{newPassword}</strong><br><br>Por favor, cámbiela después de iniciar sesión."
                ));

                return Ok(new { message = "Contraseña enviada al correo con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al recuperar contraseña", error = ex.Message });
            }
        }

        /// <summary>
        /// External RETHUS API integration for identity validation
        /// </summary>
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

        /// <summary>
        /// Generate a secure random password with mixed character types
        /// </summary>
        private string GenerarClaveSegura()
        {
            const string caracteresMinusculas = "abcdefghijklmnopqrstuvwxyz";
            const string caracteresMayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string caracteresNumeros = "0123456789";
            const string caracteresEspeciales = "!@#$%&*";
            const string todosCaracteres = caracteresMinusculas + caracteresMayusculas + caracteresNumeros + caracteresEspeciales;

            StringBuilder password = new StringBuilder();

            // Ensure at least one character of each type
            password.Append(caracteresMinusculas[RandomNumberGenerator.GetInt32(caracteresMinusculas.Length)]);
            password.Append(caracteresMayusculas[RandomNumberGenerator.GetInt32(caracteresMayusculas.Length)]);
            password.Append(caracteresNumeros[RandomNumberGenerator.GetInt32(caracteresNumeros.Length)]);
            password.Append(caracteresEspeciales[RandomNumberGenerator.GetInt32(caracteresEspeciales.Length)]);

            // Complete up to 12 characters
            for (int i = 4; i < 12; i++)
            {
                password.Append(todosCaracteres[RandomNumberGenerator.GetInt32(todosCaracteres.Length)]);
            }

            // Shuffle characters
            return new string(password.ToString().OrderBy(x => RandomNumberGenerator.GetInt32(1000)).ToArray());
        }
    }
}
