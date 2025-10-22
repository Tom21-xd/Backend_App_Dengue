namespace Backend_App_Dengue.Services
{
    public static class EmailTemplates
    {
        /// <summary>
        /// Genera una plantilla HTML moderna para el correo de recuperación de contraseña
        /// </summary>
        public static string RecoverPasswordTemplate(string newPassword, string userEmail)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Recuperación de Contraseña - Dengue Track</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table role='presentation' style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td style='padding: 40px 0; text-align: center;'>
                <table role='presentation' style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>

                    <!-- Header con gradiente verde -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #00C853 0%, #69F0AE 100%); padding: 40px 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                            <img src='https://api.prometeondev.com/images/logo-dengue-track.png' alt='Dengue Track' style='width: 100px; height: 100px; margin-bottom: 20px;' />
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: bold;'>Dengue Track</h1>
                            <p style='color: #ffffff; margin: 10px 0 0 0; font-size: 14px; opacity: 0.9;'>Sistema de Monitoreo - UCEVA</p>
                        </td>
                    </tr>

                    <!-- Contenido principal -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <h2 style='color: #1a237e; margin: 0 0 20px 0; font-size: 24px;'>Recuperación de Contraseña</h2>

                            <p style='color: #555555; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                Hola,
                            </p>

                            <p style='color: #555555; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>
                                Hemos recibido una solicitud para recuperar tu contraseña. Tu nueva contraseña temporal es:
                            </p>

                            <!-- Caja de contraseña -->
                            <table role='presentation' style='width: 100%; border-collapse: collapse; margin-bottom: 30px;'>
                                <tr>
                                    <td style='background-color: #f5f5f5; border: 2px dashed #00C853; border-radius: 8px; padding: 20px; text-align: center;'>
                                        <p style='color: #666666; font-size: 12px; margin: 0 0 10px 0; text-transform: uppercase; letter-spacing: 1px;'>Tu Nueva Contraseña</p>
                                        <p style='color: #1a237e; font-size: 24px; font-weight: bold; margin: 0; letter-spacing: 2px; font-family: Courier, monospace;'>{newPassword}</p>
                                    </td>
                                </tr>
                            </table>

                            <!-- Instrucciones de seguridad -->
                            <div style='background-color: #fff3e0; border-left: 4px solid #ff9800; padding: 15px; margin-bottom: 30px; border-radius: 4px;'>
                                <p style='color: #e65100; font-size: 14px; margin: 0 0 10px 0; font-weight: bold;'>
                                    ⚠️ Importante - Seguridad
                                </p>
                                <ul style='color: #666666; font-size: 14px; line-height: 1.6; margin: 0; padding-left: 20px;'>
                                    <li>Por favor, cambia esta contraseña después de iniciar sesión</li>
                                    <li>No compartas esta contraseña con nadie</li>
                                    <li>Si no solicitaste este cambio, contacta al administrador</li>
                                </ul>
                            </div>

                            <!-- Botón de acción -->
                            <table role='presentation' style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='text-align: center; padding: 20px 0;'>
                                        <a href='https://api.prometeondev.com' style='background: linear-gradient(135deg, #00C853 0%, #69F0AE 100%); color: #ffffff; padding: 15px 40px; text-decoration: none; border-radius: 25px; font-size: 16px; font-weight: bold; display: inline-block; box-shadow: 0 4px 6px rgba(0,200,83,0.3);'>
                                            Abrir Dengue Track
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <p style='color: #999999; font-size: 13px; line-height: 1.6; margin: 30px 0 0 0; text-align: center;'>
                                Cuenta asociada: <strong>{userEmail}</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #1a237e; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;'>
                            <p style='color: #ffffff; font-size: 14px; margin: 0 0 10px 0;'>
                                <strong>Universidad Central del Valle del Cauca</strong>
                            </p>
                            <p style='color: #b0bec5; font-size: 12px; margin: 0 0 15px 0;'>
                                Sistema de Monitoreo y Gestión de Dengue
                            </p>
                            <div style='border-top: 1px solid rgba(255,255,255,0.1); padding-top: 15px; margin-top: 15px;'>
                                <p style='color: #90a4ae; font-size: 11px; margin: 0;'>
                                    Este es un correo automático, por favor no responder.<br/>
                                    © {DateTime.Now.Year} UCEVA - Todos los derechos reservados
                                </p>
                            </div>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        /// <summary>
        /// Genera una plantilla HTML para correos de bienvenida
        /// </summary>
        public static string WelcomeTemplate(string userName, string userEmail)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bienvenido a Dengue Track</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table role='presentation' style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td style='padding: 40px 0; text-align: center;'>
                <table role='presentation' style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>

                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #00C853 0%, #69F0AE 100%); padding: 40px 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                            <img src='https://api.prometeondev.com/images/logo-dengue-track.png' alt='Dengue Track' style='width: 100px; height: 100px; margin-bottom: 20px;' />
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: bold;'>¡Bienvenido a Dengue Track!</h1>
                        </td>
                    </tr>

                    <!-- Contenido -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <h2 style='color: #1a237e; margin: 0 0 20px 0; font-size: 24px;'>Hola, {userName} 👋</h2>

                            <p style='color: #555555; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                Tu cuenta ha sido creada exitosamente en <strong>Dengue Track</strong>, el sistema de monitoreo y gestión de dengue de UCEVA.
                            </p>

                            <div style='background-color: #e8f5e9; border-left: 4px solid #00C853; padding: 20px; margin: 30px 0; border-radius: 4px;'>
                                <p style='color: #1b5e20; font-size: 15px; margin: 0; line-height: 1.6;'>
                                    <strong>✓</strong> Ya puedes acceder a todas las funcionalidades del sistema<br/>
                                    <strong>✓</strong> Monitorear casos de dengue en tiempo real<br/>
                                    <strong>✓</strong> Recibir notificaciones importantes<br/>
                                    <strong>✓</strong> Contribuir a la prevención del dengue
                                </p>
                            </div>

                            <table role='presentation' style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='text-align: center; padding: 20px 0;'>
                                        <a href='https://api.prometeondev.com' style='background: linear-gradient(135deg, #00C853 0%, #69F0AE 100%); color: #ffffff; padding: 15px 40px; text-decoration: none; border-radius: 25px; font-size: 16px; font-weight: bold; display: inline-block; box-shadow: 0 4px 6px rgba(0,200,83,0.3);'>
                                            Iniciar Sesión
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <p style='color: #999999; font-size: 13px; line-height: 1.6; margin: 30px 0 0 0; text-align: center;'>
                                Cuenta: <strong>{userEmail}</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #1a237e; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;'>
                            <p style='color: #ffffff; font-size: 14px; margin: 0 0 10px 0;'>
                                <strong>Universidad Central del Valle del Cauca</strong>
                            </p>
                            <p style='color: #b0bec5; font-size: 12px; margin: 0 0 15px 0;'>
                                Sistema de Monitoreo y Gestión de Dengue
                            </p>
                            <div style='border-top: 1px solid rgba(255,255,255,0.1); padding-top: 15px; margin-top: 15px;'>
                                <p style='color: #90a4ae; font-size: 11px; margin: 0;'>
                                    © {DateTime.Now.Year} UCEVA - Todos los derechos reservados
                                </p>
                            </div>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
