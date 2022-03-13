using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace Comet.Web.Services
{
    public class EmailService : IEmailSender
    {
        private readonly ILogger<EmailService> mLogger;
        private readonly EmailSettings mSettings;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            mSettings = new EmailSettings
            {
                PrimaryDomain = configuration["DontReplyMail:PrimaryDomain"],
                PrimaryPort = int.Parse(configuration["DontReplyMail:PrimaryPort"]),
                UsernameEmail = configuration["DontReplyMail:UsernameEmail"],
                UsernamePassword = configuration["DontReplyMail:UsernamePassword"],
                FromEmail = configuration["DontReplyMail:FromEmail"],
                EnableSsl = bool.Parse(configuration["DontReplyMail:EnableSsl"]),
                FromName = configuration["DontReplyMail:FromName"]
            };

            mLogger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mSettings.FromName, mSettings.FromEmail));
            message.To.Add(new MailboxAddress(email, email));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage
            };
            try
            {
                using var client = new SmtpClient();
                /**
                 * Keep this lines if using Selfsigned certificate (or no certificate at all)
                 */
                client.ServerCertificateValidationCallback = (s, c, h, e) => true; //NOSONAR
                client.CheckCertificateRevocation = false;                         //NOSONAR

                await client.ConnectAsync(mSettings.PrimaryDomain, mSettings.PrimaryPort);
                await client.AuthenticateAsync(mSettings.UsernameEmail, mSettings.UsernamePassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex.ToString());
            }
        }

        public class EmailSettings
        {
            public string PrimaryDomain { get; set; }
            public int PrimaryPort { get; set; }
            public string UsernameEmail { get; set; }
            public string UsernamePassword { get; set; }
            public string FromName { get; set; }
            public string FromEmail { get; set; }
            public bool EnableSsl { get; set; }
        }
    }
}