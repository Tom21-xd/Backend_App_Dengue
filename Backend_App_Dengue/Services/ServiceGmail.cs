using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;


namespace Backend_App_Dengue.Services
{
    public class ServiceGmail
    {
        public void SendEmailGmail(String receptor, String asunto, String mensaje)
        {
            var aux = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            MimeMessage mail = new MimeMessage();

            String usermail = aux.GetSection("gmail:usuariogmail").Value;

            String passwordmail = aux.GetSection("gmail:passwordgmail").Value;

            mail.From.Add(MailboxAddress.Parse(usermail));
            mail.To.Add(MailboxAddress.Parse(receptor));
            mail.Subject = asunto;
            mail.Body = new TextPart(TextFormat.Html)
            {
                Text = mensaje
            };
            String smtpserver = aux.GetSection("gmail:hostGmail").Value;
            int port = int.Parse(aux.GetSection("gmail:portGmail").Value);
            bool ssl = bool.Parse(aux.GetSection("gmail:sslGmail").Value);
            bool defaultcreadentials = bool.Parse(aux.GetSection("gmail:defaultcredentialsGmail").Value);

            using var smtpClient = new SmtpClient();
            smtpClient.Connect(
                smtpserver,
                port,
                SecureSocketOptions.StartTls
                );
            smtpClient.Authenticate(usermail, passwordmail);
            smtpClient.Send(mail);
            smtpClient.Disconnect(true);
        }

    }
}
