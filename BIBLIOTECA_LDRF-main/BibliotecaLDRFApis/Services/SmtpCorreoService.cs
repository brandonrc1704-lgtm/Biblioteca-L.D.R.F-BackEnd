using Biblioteca.Dominio.InterfaceLN;
using System.Net;
using System.Net.Mail;

namespace BibliotecaLDRFApis.Services
{
    public class SmtpCorreoService : ICorreoService
    {
        private readonly IConfiguration _configuration;

        public SmtpCorreoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarAsync(string destino, string asunto, string mensaje)
        {
            var host = _configuration["Smtp:Host"];
            var user = _configuration["Smtp:User"];
            var password = _configuration["Smtp:Password"];
            var from = _configuration["Smtp:From"] ?? user;
            var port = int.TryParse(_configuration["Smtp:Port"], out var configuredPort)
                ? configuredPort
                : 587;
            var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var configuredSsl) || configuredSsl;

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(from))
            {
                throw new InvalidOperationException("El servicio SMTP no esta configurado.");
            }

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(user, password)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(from, "Biblioteca C.D.R.F"),
                Subject = asunto,
                Body = mensaje,
                IsBodyHtml = false
            };
            mail.To.Add(destino);

            await client.SendMailAsync(mail);
        }
    }
}
