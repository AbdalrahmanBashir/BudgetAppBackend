using BudgetAppBackend.Application.Models;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Domain.DomainEvents;
using MediatR;

namespace BudgetAppBackend.Infrastructure.EventHandlers
{
    public class PasswordChangedEventHandler : INotificationHandler<PasswordChangedEvent>
    {
        private readonly IEmailService _emailService;

        public PasswordChangedEventHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Handle(PasswordChangedEvent notification, CancellationToken cancellationToken)
        {
            string emailBody = $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f4f4;
                        margin: 0;
                        padding: 0;
                    }}
                    .email-container {{
                        max-width: 600px;
                        margin: 20px auto;
                        background-color: #ffffff;
                        border-radius: 8px;
                        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                        overflow: hidden;
                    }}
                    .email-header {{
                        background-color: #007BFF;
                        color: #ffffff;
                        text-align: center;
                        padding: 20px;
                        font-size: 24px;
                        font-weight: bold;
                    }}
                    .email-body {{
                        padding: 20px;
                        font-size: 16px;
                        color: #333333;
                    }}
                    .email-body p {{
                        margin: 0 0 10px;
                        line-height: 1.6;
                    }}
                    .email-footer {{
                        background-color: #f8f8f8;
                        padding: 15px;
                        text-align: center;
                        font-size: 12px;
                        color: #888888;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='email-header'>Password Changed Successfully</div>
                    <div class='email-body'>
                        <p>Dear User,</p>
                        <p>Your password has been successfully changed. If you did not request this change, please contact support immediately.</p>
                        <p>Time of change: <strong>{DateTime.UtcNow:dddd, MMMM d, yyyy h:mm tt UTC}</strong></p>
                        <p>If you have any concerns, please contact our support team.</p>
                    </div>
                    <div class='email-footer'>
                        &copy; {DateTime.UtcNow.Year} BudgetApp. All rights reserved.
                    </div>
                </div>
            </body>
            </html>";

            var emailMessage = new EmailMessage(
                notification.Email,
                "Password Change Notification",
                emailBody,
                notification.LastName
            );

            try
            {
                await _emailService.SendEmail(emailMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send password change email: {ex.Message}");
            }
        }
    }
}
