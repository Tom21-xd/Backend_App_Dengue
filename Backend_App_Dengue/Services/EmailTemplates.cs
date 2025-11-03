using System;

namespace Backend_App_Dengue.Services
{
    public static class EmailTemplates
    {
        /// <summary>
        /// Genera una plantilla HTML compatible para el correo de recuperación de contraseña
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
<body style='margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background-color: #f4f7fa;'>

    <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background-color: #5E81F4; padding: 40px 0;'>
        <tr>
            <td align='center'>

                <!-- Contenedor principal -->
                <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='600' style='max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.1);'>

                    <!-- Header con gradiente azul -->
                    <tr>
                        <td style='background-color: #5E81F4; padding: 50px 30px; text-align: center;'>
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td align='center'>
                                        <div style='width: 100px; height: 100px; background-color: rgba(255,255,255,0.2); border-radius: 50%; margin: 0 auto 20px; text-align: center; line-height: 100px;'>
                                            <span style='font-size: 50px;'>🔐</span>
                                        </div>
                                        <h1 style='color: #ffffff; margin: 0 0 10px 0; font-size: 32px; font-weight: bold;'>Recupera tu Acceso</h1>
                                        <p style='color: #ffffff; margin: 0; font-size: 16px; opacity: 0.95;'>Tu nueva contraseña está lista</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Contenido -->
                    <tr>
                        <td style='padding: 40px 30px;'>

                            <!-- Saludo -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='text-align: center; padding-bottom: 30px;'>
                                        <h2 style='color: #1a202c; margin: 0 0 10px 0; font-size: 24px; font-weight: bold;'>¡Hola! 👋</h2>
                                        <p style='color: #4a5568; font-size: 15px; line-height: 1.6; margin: 0;'>
                                            Recibimos tu solicitud de recuperación de contraseña.<br/>
                                            Generamos una nueva contraseña segura para ti.
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <!-- Tarjeta de contraseña -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 30px;'>
                                <tr>
                                    <td style='background-color: #f8fafc; border: 2px solid #5E81F4; border-radius: 12px; padding: 25px; text-align: center;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='padding-bottom: 15px;'>
                                                    <span style='font-size: 20px; margin-right: 8px;'>🔑</span>
                                                    <span style='color: #5E81F4; font-size: 12px; text-transform: uppercase; letter-spacing: 1.5px; font-weight: bold;'>Tu Contraseña Temporal</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='background-color: #ffffff; border-radius: 8px; padding: 20px;'>
                                                    <p style='color: #1a202c; font-size: 28px; font-weight: bold; margin: 0; letter-spacing: 3px; font-family: Courier, monospace; word-break: break-all;'>{newPassword}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding-top: 10px;'>
                                                    <p style='color: #718096; font-size: 12px; margin: 0; font-style: italic;'>💡 Copia exactamente como aparece arriba</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Alerta de seguridad -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 30px;'>
                                <tr>
                                    <td style='background-color: #fff7ed; border-left: 4px solid #FFB946; border-radius: 8px; padding: 20px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='padding-bottom: 12px;'>
                                                    <span style='font-size: 22px; margin-right: 8px;'>⚠️</span>
                                                    <span style='color: #ea8d0a; font-size: 16px; font-weight: bold;'>Importante: Seguridad de tu Cuenta</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='color: #7d5a29; font-size: 14px; line-height: 1.7;'>
                                                    <p style='margin: 0 0 8px 0;'>🔸 <strong>Cambia esta contraseña</strong> inmediatamente después de iniciar sesión</p>
                                                    <p style='margin: 0 0 8px 0;'>🔸 <strong>Nunca compartas</strong> tu contraseña con nadie</p>
                                                    <p style='margin: 0 0 8px 0;'>🔸 <strong>Elimina este correo</strong> tras actualizar tu contraseña</p>
                                                    <p style='margin: 0;'>🔸 Si <strong>no solicitaste este cambio</strong>, contacta inmediatamente al administrador</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Pasos -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 30px;'>
                                <tr>
                                    <td style='background-color: #f7fafc; border-radius: 12px; padding: 25px;'>
                                        <h3 style='color: #1a202c; font-size: 18px; margin: 0 0 20px 0; font-weight: bold; text-align: center;'>📋 Pasos para Recuperar tu Cuenta</h3>

                                        <!-- Paso 1 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 15px;'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 32px; height: 32px; background-color: #5E81F4; border-radius: 50%; text-align: center; line-height: 32px; color: #ffffff; font-weight: bold; font-size: 16px;'>1</div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #2d3748; font-size: 14px; margin: 6px 0 0 0; line-height: 1.5;'>Abre la aplicación <strong>Dengue Track</strong> en tu dispositivo</p>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Paso 2 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 15px;'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 32px; height: 32px; background-color: #5E81F4; border-radius: 50%; text-align: center; line-height: 32px; color: #ffffff; font-weight: bold; font-size: 16px;'>2</div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #2d3748; font-size: 14px; margin: 6px 0 0 0; line-height: 1.5;'>Inicia sesión con tu correo y la <strong>contraseña temporal</strong> de arriba</p>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Paso 3 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 32px; height: 32px; background-color: #5E81F4; border-radius: 50%; text-align: center; line-height: 32px; color: #ffffff; font-weight: bold; font-size: 16px;'>3</div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #2d3748; font-size: 14px; margin: 6px 0 0 0; line-height: 1.5;'>Ve a <strong>Configuración</strong> y cambia tu contraseña por una nueva y segura</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Botón -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 25px;'>
                                <tr>
                                    <td align='center'>
                                        <a href='https://api.prometeondev.com' style='display: inline-block; background-color: #5E81F4; color: #ffffff; padding: 16px 40px; text-decoration: none; border-radius: 30px; font-size: 16px; font-weight: bold;'>🚀 Abrir Dengue Track</a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Info de cuenta -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='border-top: 2px solid #e2e8f0; padding-top: 20px; text-align: center;'>
                                        <p style='color: #a0aec0; font-size: 13px; margin: 0;'>
                                            Cuenta asociada: <strong style='color: #4a5568;'>{userEmail}</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #2d3748; padding: 30px; text-align: center;'>
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td>
                                        <p style='color: #ffffff; font-size: 16px; margin: 0 0 6px 0; font-weight: bold;'>
                                            Universidad Central del Valle del Cauca
                                        </p>
                                        <p style='color: #cbd5e0; font-size: 13px; margin: 0 0 20px 0;'>
                                            Sistema de Monitoreo y Gestión de Dengue
                                        </p>
                                        <p style='color: #a0aec0; font-size: 11px; margin: 0 0 4px 0;'>
                                            Este es un correo automático, por favor no responder
                                        </p>
                                        <p style='color: #718096; font-size: 11px; margin: 0;'>
                                            © {DateTime.Now.Year} UCEVA - Todos los derechos reservados
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                </table>

                <!-- Nota de privacidad -->
                <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='600' style='max-width: 600px; padding-top: 20px;'>
                    <tr>
                        <td align='center'>
                            <p style='color: #ffffff; font-size: 11px; margin: 0; line-height: 1.5; opacity: 0.9;'>
                                🔒 Este mensaje fue enviado a {userEmail} como parte del proceso de recuperación de contraseña de Dengue Track.
                            </p>
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
        /// Genera una plantilla HTML compatible para correos de bienvenida
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
<body style='margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background-color: #f4f7fa;'>

    <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background-color: #26DE81; padding: 40px 0;'>
        <tr>
            <td align='center'>

                <!-- Contenedor principal -->
                <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='600' style='max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.1);'>

                    <!-- Header -->
                    <tr>
                        <td style='background-color: #5E81F4; padding: 40px 30px; text-align: center;'>

                            <!-- Logos institucionales -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 30px;'>
                                <tr>
                                    <td align='center' style='background-color: rgba(255,255,255,0.1); border-radius: 12px; padding: 15px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0'>
                                            <tr>
                                                <td align='center' style='padding: 0 10px;'>
                                                    <img src='https://api.prometeondev.com/images/uceva.png' alt='UCEVA' width='50' height='50' style='display: block; filter: brightness(0) invert(1);' />
                                                </td>
                                                <td align='center' style='padding: 0 10px;'>
                                                    <img src='https://api.prometeondev.com/images/uniamazonia.png' alt='Universidad de la Amazonía' width='50' height='50' style='display: block; filter: brightness(0) invert(1);' />
                                                </td>
                                                <td align='center' style='padding: 0 10px;'>
                                                    <img src='https://api.prometeondev.com/images/minciencias.png' alt='Minciencias' width='50' height='50' style='display: block; filter: brightness(0) invert(1);' />
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Icono principal -->
                            <div style='width: 100px; height: 100px; background-color: rgba(255,255,255,0.2); border-radius: 50%; margin: 0 auto 20px; text-align: center; line-height: 100px;'>
                                <span style='font-size: 50px;'>🦟</span>
                            </div>

                            <h1 style='color: #ffffff; margin: 0 0 10px 0; font-size: 32px; font-weight: bold;'>¡Bienvenido a Dengue Track!</h1>
                            <p style='color: #ffffff; margin: 0; font-size: 15px; opacity: 0.95;'>Sistema de Vigilancia Epidemiológica del Dengue</p>
                        </td>
                    </tr>

                    <!-- Contenido -->
                    <tr>
                        <td style='padding: 40px 30px;'>

                            <!-- Saludo personalizado -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='text-align: center; padding-bottom: 30px;'>
                                        <div style='display: inline-block; background-color: #26DE81; padding: 6px 20px; border-radius: 20px; margin-bottom: 15px;'>
                                            <span style='color: #ffffff; font-size: 11px; font-weight: bold; text-transform: uppercase; letter-spacing: 0.5px;'>🎉 Cuenta Activada</span>
                                        </div>
                                        <h2 style='color: #1a202c; margin: 0 0 12px 0; font-size: 26px; font-weight: bold;'>¡Hola, {userName}! 👋</h2>
                                        <p style='color: #4a5568; font-size: 15px; line-height: 1.6; margin: 0;'>
                                            Tu cuenta ha sido creada exitosamente. Ya eres parte del equipo que combate el dengue en nuestra región.
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <!-- Funcionalidades -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 30px;'>
                                <tr>
                                    <td style='background-color: #ecfdf5; border: 2px solid #26DE81; border-radius: 12px; padding: 25px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td align='center' style='padding-bottom: 20px;'>
                                                    <div style='display: inline-block; background-color: #26DE81; padding: 8px 20px; border-radius: 20px;'>
                                                        <span style='font-size: 13px; font-weight: bold; color: #ffffff;'>🎯 TUS HERRAMIENTAS</span>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Feature 1 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 12px;'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 36px; height: 36px; background-color: #26DE81; border-radius: 10px; text-align: center; line-height: 36px;'>
                                                        <span style='font-size: 20px;'>📊</span>
                                                    </div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #065f46; font-size: 14px; margin: 8px 0 0 0; line-height: 1.5;'><strong>Monitoreo en tiempo real</strong> de casos de dengue en tu región</p>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Feature 2 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 12px;'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 36px; height: 36px; background-color: #26DE81; border-radius: 10px; text-align: center; line-height: 36px;'>
                                                        <span style='font-size: 20px;'>🗺️</span>
                                                    </div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #065f46; font-size: 14px; margin: 8px 0 0 0; line-height: 1.5;'><strong>Mapas de calor</strong> y visualización geográfica interactiva</p>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Feature 3 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 12px;'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 36px; height: 36px; background-color: #26DE81; border-radius: 10px; text-align: center; line-height: 36px;'>
                                                        <span style='font-size: 20px;'>🔔</span>
                                                    </div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #065f46; font-size: 14px; margin: 8px 0 0 0; line-height: 1.5;'><strong>Alertas inteligentes</strong> sobre brotes epidemiológicos</p>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Feature 4 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 12px;'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 36px; height: 36px; background-color: #26DE81; border-radius: 10px; text-align: center; line-height: 36px;'>
                                                        <span style='font-size: 20px;'>📈</span>
                                                    </div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #065f46; font-size: 14px; margin: 8px 0 0 0; line-height: 1.5;'><strong>Reportes detallados</strong> y análisis estadístico avanzado</p>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Feature 5 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 36px; height: 36px; background-color: #26DE81; border-radius: 10px; text-align: center; line-height: 36px;'>
                                                        <span style='font-size: 20px;'>📚</span>
                                                    </div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #065f46; font-size: 14px; margin: 8px 0 0 0; line-height: 1.5;'><strong>Educación continua</strong> sobre prevención del dengue</p>
                                                </td>
                                            </tr>
                                        </table>

                                    </td>
                                </tr>
                            </table>

                            <!-- Primeros pasos -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 30px;'>
                                <tr>
                                    <td style='background-color: #f7fafc; border-radius: 12px; padding: 25px;'>
                                        <h3 style='color: #1a202c; font-size: 18px; margin: 0 0 20px 0; font-weight: bold; text-align: center;'>🚀 Comienza tu Experiencia</h3>

                                        <!-- Paso 1 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 15px;'>
                                            <tr>
                                                <td width='45' valign='top'>
                                                    <div style='width: 38px; height: 38px; background-color: #5E81F4; border-radius: 50%; text-align: center; line-height: 38px; color: #ffffff; font-weight: bold; font-size: 18px;'>1</div>
                                                </td>
                                                <td valign='top' style='padding-left: 15px;'>
                                                    <p style='color: #2d3748; font-size: 14px; margin: 8px 0 0 0; line-height: 1.5;'><strong>Descarga</strong> la aplicación móvil Dengue Track</p>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Paso 2 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 15px;'>
                                            <tr>
                                                <td width='45' valign='top'>
                                                    <div style='width: 38px; height: 38px; background-color: #5E81F4; border-radius: 50%; text-align: center; line-height: 38px; color: #ffffff; font-weight: bold; font-size: 18px;'>2</div>
                                                </td>
                                                <td valign='top' style='padding-left: 15px;'>
                                                    <p style='color: #2d3748; font-size: 14px; margin: 8px 0 0 0; line-height: 1.5;'><strong>Inicia sesión</strong> con tu correo y contraseña</p>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Paso 3 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td width='45' valign='top'>
                                                    <div style='width: 38px; height: 38px; background-color: #5E81F4; border-radius: 50%; text-align: center; line-height: 38px; color: #ffffff; font-weight: bold; font-size: 18px;'>3</div>
                                                </td>
                                                <td valign='top' style='padding-left: 15px;'>
                                                    <p style='color: #2d3748; font-size: 14px; margin: 8px 0 0 0; line-height: 1.5;'><strong>Explora</strong> el mapa y todas las funcionalidades</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Botón -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 25px;'>
                                <tr>
                                    <td align='center'>
                                        <a href='https://api.prometeondev.com' style='display: inline-block; background-color: #26DE81; color: #ffffff; padding: 16px 45px; text-decoration: none; border-radius: 30px; font-size: 16px; font-weight: bold;'>📱 Acceder Ahora</a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Info de cuenta -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='border-top: 2px solid #e2e8f0; padding-top: 20px; text-align: center;'>
                                        <p style='color: #a0aec0; font-size: 13px; margin: 0;'>
                                            Tu cuenta: <strong style='color: #4a5568;'>{userEmail}</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #5E81F4; padding: 30px; text-align: center;'>
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td>
                                        <p style='color: #ffffff; font-size: 16px; margin: 0 0 6px 0; font-weight: bold;'>
                                            Universidad Central del Valle del Cauca
                                        </p>
                                        <p style='color: rgba(255,255,255,0.9); font-size: 13px; margin: 0 0 15px 0;'>
                                            En colaboración con Universidad de la Amazonía y Minciencias
                                        </p>
                                        <div style='background-color: rgba(255,255,255,0.1); border-radius: 10px; padding: 15px; margin-bottom: 15px;'>
                                            <p style='color: #ffffff; font-size: 14px; margin: 0 0 3px 0; font-weight: bold;'>Dengue Track</p>
                                            <p style='color: rgba(255,255,255,0.9); font-size: 12px; margin: 0;'>Sistema de Vigilancia Epidemiológica del Dengue</p>
                                        </div>
                                        <p style='color: rgba(255,255,255,0.8); font-size: 11px; margin: 0 0 4px 0;'>
                                            Este es un correo automático, por favor no responder
                                        </p>
                                        <p style='color: rgba(255,255,255,0.6); font-size: 11px; margin: 0;'>
                                            © {DateTime.Now.Year} UCEVA - Todos los derechos reservados
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                </table>

                <!-- Mensaje inspiracional -->
                <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='600' style='max-width: 600px; padding-top: 20px;'>
                    <tr>
                        <td align='center' style='background-color: rgba(255,255,255,0.1); border-radius: 10px; padding: 15px;'>
                            <p style='color: #ffffff; font-size: 12px; margin: 0; line-height: 1.5;'>
                                <strong>🤝 Juntos combatimos el dengue.</strong><br/>
                                Este proyecto es posible gracias al apoyo de UCEVA, Universidad de la Amazonía y Minciencias.
                            </p>
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
        /// Genera una plantilla HTML para notificar aprobación de cuenta con cambio de rol
        /// </summary>
        public static string ApprovalTemplate(string userName, string userEmail, string newRoleName)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Cuenta Aprobada - Dengue Track</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background-color: #f4f7fa;'>

    <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background-color: #26DE81; padding: 40px 0;'>
        <tr>
            <td align='center'>

                <!-- Contenedor principal -->
                <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='600' style='max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.1);'>

                    <!-- Header con gradiente verde -->
                    <tr>
                        <td style='background-color: #26DE81; padding: 50px 30px; text-align: center;'>
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td align='center'>
                                        <div style='width: 100px; height: 100px; background-color: rgba(255,255,255,0.2); border-radius: 50%; margin: 0 auto 20px; text-align: center; line-height: 100px;'>
                                            <span style='font-size: 50px;'>✅</span>
                                        </div>
                                        <h1 style='color: #ffffff; margin: 0 0 10px 0; font-size: 32px; font-weight: bold;'>¡Cuenta Aprobada!</h1>
                                        <p style='color: #ffffff; margin: 0; font-size: 16px; opacity: 0.95;'>Tu solicitud ha sido aceptada exitosamente</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Contenido -->
                    <tr>
                        <td style='padding: 40px 30px;'>

                            <!-- Saludo -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='text-align: center; padding-bottom: 30px;'>
                                        <h2 style='color: #1a202c; margin: 0 0 10px 0; font-size: 24px; font-weight: bold;'>¡Felicidades, {userName}! 🎉</h2>
                                        <p style='color: #4a5568; font-size: 15px; line-height: 1.6; margin: 0;'>
                                            Tu solicitud de verificación ha sido aprobada por un administrador.<br/>
                                            Ahora tienes acceso completo a todas las funcionalidades del sistema.
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <!-- Tarjeta de rol -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 30px;'>
                                <tr>
                                    <td style='background-color: #ecfdf5; border: 2px solid #26DE81; border-radius: 12px; padding: 25px; text-align: center;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='padding-bottom: 15px;'>
                                                    <span style='font-size: 20px; margin-right: 8px;'>👤</span>
                                                    <span style='color: #26DE81; font-size: 12px; text-transform: uppercase; letter-spacing: 1.5px; font-weight: bold;'>Tu Nuevo Rol</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='background-color: #ffffff; border-radius: 8px; padding: 20px;'>
                                                    <p style='color: #1a202c; font-size: 24px; font-weight: bold; margin: 0;'>{newRoleName}</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding-top: 10px;'>
                                                    <p style='color: #065f46; font-size: 12px; margin: 0; font-style: italic;'>✨ Ahora tienes permisos adicionales</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Próximos pasos -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 30px;'>
                                <tr>
                                    <td style='background-color: #f7fafc; border-radius: 12px; padding: 25px;'>
                                        <h3 style='color: #1a202c; font-size: 18px; margin: 0 0 20px 0; font-weight: bold; text-align: center;'>📋 Próximos Pasos</h3>

                                        <!-- Paso 1 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 15px;'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 32px; height: 32px; background-color: #26DE81; border-radius: 50%; text-align: center; line-height: 32px; color: #ffffff; font-weight: bold; font-size: 16px;'>1</div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #2d3748; font-size: 14px; margin: 6px 0 0 0; line-height: 1.5;'>Abre la aplicación <strong>Dengue Track</strong></p>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Paso 2 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 15px;'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 32px; height: 32px; background-color: #26DE81; border-radius: 50%; text-align: center; line-height: 32px; color: #ffffff; font-weight: bold; font-size: 16px;'>2</div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #2d3748; font-size: 14px; margin: 6px 0 0 0; line-height: 1.5;'>Inicia sesión con tus <strong>credenciales habituales</strong></p>
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Paso 3 -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td width='40' valign='top'>
                                                    <div style='width: 32px; height: 32px; background-color: #26DE81; border-radius: 50%; text-align: center; line-height: 32px; color: #ffffff; font-weight: bold; font-size: 16px;'>3</div>
                                                </td>
                                                <td valign='top' style='padding-left: 12px;'>
                                                    <p style='color: #2d3748; font-size: 14px; margin: 6px 0 0 0; line-height: 1.5;'>Explora tus <strong>nuevas funcionalidades</strong> y permisos</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Botón -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 25px;'>
                                <tr>
                                    <td align='center'>
                                        <a href='https://api.prometeondev.com' style='display: inline-block; background-color: #26DE81; color: #ffffff; padding: 16px 40px; text-decoration: none; border-radius: 30px; font-size: 16px; font-weight: bold;'>🚀 Acceder Ahora</a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Info de cuenta -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='border-top: 2px solid #e2e8f0; padding-top: 20px; text-align: center;'>
                                        <p style='color: #a0aec0; font-size: 13px; margin: 0;'>
                                            Cuenta: <strong style='color: #4a5568;'>{userEmail}</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #2d3748; padding: 30px; text-align: center;'>
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td>
                                        <p style='color: #ffffff; font-size: 16px; margin: 0 0 6px 0; font-weight: bold;'>
                                            Universidad Central del Valle del Cauca
                                        </p>
                                        <p style='color: #cbd5e0; font-size: 13px; margin: 0 0 20px 0;'>
                                            Sistema de Monitoreo y Gestión de Dengue
                                        </p>
                                        <p style='color: #a0aec0; font-size: 11px; margin: 0 0 4px 0;'>
                                            Este es un correo automático, por favor no responder
                                        </p>
                                        <p style='color: #718096; font-size: 11px; margin: 0;'>
                                            © {DateTime.Now.Year} UCEVA - Todos los derechos reservados
                                        </p>
                                    </td>
                                </tr>
                            </table>
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
        /// Genera una plantilla HTML para notificar rechazo de solicitud de aprobación
        /// </summary>
        public static string RejectionTemplate(string userName, string userEmail, string rejectionReason)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Actualización de Solicitud - Dengue Track</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background-color: #f4f7fa;'>

    <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background-color: #FFB946; padding: 40px 0;'>
        <tr>
            <td align='center'>

                <!-- Contenedor principal -->
                <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='600' style='max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.1);'>

                    <!-- Header con gradiente naranja -->
                    <tr>
                        <td style='background-color: #FFB946; padding: 50px 30px; text-align: center;'>
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td align='center'>
                                        <div style='width: 100px; height: 100px; background-color: rgba(255,255,255,0.2); border-radius: 50%; margin: 0 auto 20px; text-align: center; line-height: 100px;'>
                                            <span style='font-size: 50px;'>📋</span>
                                        </div>
                                        <h1 style='color: #ffffff; margin: 0 0 10px 0; font-size: 32px; font-weight: bold;'>Actualización de Solicitud</h1>
                                        <p style='color: #ffffff; margin: 0; font-size: 16px; opacity: 0.95;'>Información sobre tu solicitud de verificación</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Contenido -->
                    <tr>
                        <td style='padding: 40px 30px;'>

                            <!-- Saludo -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='text-align: center; padding-bottom: 30px;'>
                                        <h2 style='color: #1a202c; margin: 0 0 10px 0; font-size: 24px; font-weight: bold;'>Hola, {userName}</h2>
                                        <p style='color: #4a5568; font-size: 15px; line-height: 1.6; margin: 0;'>
                                            Lamentamos informarte que tu solicitud de verificación no pudo ser aprobada en este momento.
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <!-- Tarjeta de motivo -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 30px;'>
                                <tr>
                                    <td style='background-color: #fff7ed; border: 2px solid #FFB946; border-radius: 12px; padding: 25px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='padding-bottom: 15px;'>
                                                    <span style='font-size: 20px; margin-right: 8px;'>📝</span>
                                                    <span style='color: #ea8d0a; font-size: 12px; text-transform: uppercase; letter-spacing: 1.5px; font-weight: bold;'>Motivo del Rechazo</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='background-color: #ffffff; border-radius: 8px; padding: 20px;'>
                                                    <p style='color: #1a202c; font-size: 15px; margin: 0; line-height: 1.6;'>{rejectionReason}</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Información importante -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 30px;'>
                                <tr>
                                    <td style='background-color: #eff6ff; border-left: 4px solid #5E81F4; border-radius: 8px; padding: 20px;'>
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                            <tr>
                                                <td style='padding-bottom: 12px;'>
                                                    <span style='font-size: 22px; margin-right: 8px;'>ℹ️</span>
                                                    <span style='color: #1e40af; font-size: 16px; font-weight: bold;'>Información Importante</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='color: #1e3a8a; font-size: 14px; line-height: 1.7;'>
                                                    <p style='margin: 0 0 8px 0;'>🔹 Tu cuenta sigue <strong>activa</strong> con permisos de usuario básico</p>
                                                    <p style='margin: 0 0 8px 0;'>🔹 Puedes <strong>seguir usando</strong> la aplicación con normalidad</p>
                                                    <p style='margin: 0 0 8px 0;'>🔹 Si necesitas permisos adicionales, corrige la información y <strong>solicita de nuevo</strong></p>
                                                    <p style='margin: 0;'>🔹 Para más información, <strong>contacta al administrador</strong></p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Botón -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='margin-bottom: 25px;'>
                                <tr>
                                    <td align='center'>
                                        <a href='https://api.prometeondev.com' style='display: inline-block; background-color: #5E81F4; color: #ffffff; padding: 16px 40px; text-decoration: none; border-radius: 30px; font-size: 16px; font-weight: bold;'>📱 Abrir Aplicación</a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Info de cuenta -->
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td style='border-top: 2px solid #e2e8f0; padding-top: 20px; text-align: center;'>
                                        <p style='color: #a0aec0; font-size: 13px; margin: 0;'>
                                            Cuenta: <strong style='color: #4a5568;'>{userEmail}</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #2d3748; padding: 30px; text-align: center;'>
                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%'>
                                <tr>
                                    <td>
                                        <p style='color: #ffffff; font-size: 16px; margin: 0 0 6px 0; font-weight: bold;'>
                                            Universidad Central del Valle del Cauca
                                        </p>
                                        <p style='color: #cbd5e0; font-size: 13px; margin: 0 0 20px 0;'>
                                            Sistema de Monitoreo y Gestión de Dengue
                                        </p>
                                        <p style='color: #a0aec0; font-size: 11px; margin: 0 0 4px 0;'>
                                            Este es un correo automático, por favor no responder
                                        </p>
                                        <p style='color: #718096; font-size: 11px; margin: 0;'>
                                            © {DateTime.Now.Year} UCEVA - Todos los derechos reservados
                                        </p>
                                    </td>
                                </tr>
                            </table>
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
