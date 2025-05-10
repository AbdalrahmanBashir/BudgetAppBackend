using MailKit.Net.Smtp;
using MailKit.Security;
using BudgetAppBackend.Application.Models;
using BudgetAppBackend.Application.Service;
using Microsoft.Extensions.Options;
using MimeKit;
using BudgetAppBackend.Application.Configuration;

namespace BudgetAppBackend.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendEmail(EmailMessage email, CancellationToken cancellationToken)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            emailMessage.To.Add(new MailboxAddress(email.ToName, email.To));
            emailMessage.Subject = email.Subject;

            emailMessage.Body = new TextPart("Html") { Text = email.Body };


            var client = new SmtpClient();
            client.Connect(_emailSettings.SmtpServer, _emailSettings.SmtpPort ?? 0, SecureSocketOptions.StartTls);
            client.Authenticate(_emailSettings.SenderEmail, _emailSettings.EmailPassword);
            client.Send(emailMessage);
            client.Disconnect(true);
            return true;
        }
    }
}
