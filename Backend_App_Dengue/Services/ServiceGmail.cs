using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;

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
    }
}
