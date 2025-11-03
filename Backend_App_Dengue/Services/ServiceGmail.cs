using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit.Utils;

namespace Backend_App_Dengue.Services
{
    public class ServiceGmail
    {
        private readonly string _userEmail;
        private readonly string _password;
        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableSsl;

        public ServiceGmail()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _userEmail = config["Gmail:UserEmail"]
                ?? throw new InvalidOperationException("Gmail UserEmail not configured");

            _password = config["Gmail:Password"]
                ?? throw new InvalidOperationException("Gmail Password not configured");

            _host = config["Gmail:Host"]
                ?? throw new InvalidOperationException("Gmail Host not configured");

            _port = int.Parse(config["Gmail:Port"] ?? "587");
            _enableSsl = bool.Parse(config["Gmail:EnableSsl"] ?? "true");
        }

        public void SendEmailGmail(string receptor, string asunto, string mensaje)
        {
            if (string.IsNullOrEmpty(receptor))
            {
                throw new ArgumentException("El email del receptor es requerido", nameof(receptor));
            }

            try
            {
                MimeMessage mail = new MimeMessage();

                mail.From.Add(MailboxAddress.Parse(_userEmail));
                mail.To.Add(MailboxAddress.Parse(receptor));
                mail.Subject = asunto;
                mail.Body = new TextPart(TextFormat.Html)
                {
                    Text = mensaje
                };

                using var smtpClient = new SmtpClient();
                smtpClient.Connect(_host, _port, SecureSocketOptions.StartTls);
                smtpClient.Authenticate(_userEmail, _password);
                smtpClient.Send(mail);
                smtpClient.Disconnect(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar el correo a {receptor}", ex);
            }
        }

        /// <summary>
        /// Envía un correo electrónico con archivo PDF adjunto
        /// </summary>
        /// <param name="receptor">Email del destinatario</param>
        /// <param name="asunto">Asunto del correo</param>
        /// <param name="mensaje">Cuerpo del mensaje en formato HTML</param>
        /// <param name="pdfBytes">Bytes del archivo PDF a adjuntar</param>
        /// <param name="pdfFileName">Nombre del archivo PDF</param>
        public void SendEmailWithPdfAttachment(string receptor, string asunto, string mensaje, byte[] pdfBytes, string pdfFileName)
        {
            if (string.IsNullOrEmpty(receptor))
            {
                throw new ArgumentException("El email del receptor es requerido", nameof(receptor));
            }

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new ArgumentException("El archivo PDF es requerido", nameof(pdfBytes));
            }

            try
            {
                MimeMessage mail = new MimeMessage();

                mail.From.Add(MailboxAddress.Parse(_userEmail));
                mail.To.Add(MailboxAddress.Parse(receptor));
                mail.Subject = asunto;

                // Crear el cuerpo del mensaje con HTML y el archivo adjunto
                var builder = new BodyBuilder
                {
                    HtmlBody = mensaje
                };

                // Agregar el PDF como adjunto
                builder.Attachments.Add(pdfFileName, pdfBytes, new ContentType("application", "pdf"));

                mail.Body = builder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                smtpClient.Connect(_host, _port, SecureSocketOptions.StartTls);
                smtpClient.Authenticate(_userEmail, _password);
                smtpClient.Send(mail);
                smtpClient.Disconnect(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar el correo con adjunto a {receptor}", ex);
            }
        }
    }
}
