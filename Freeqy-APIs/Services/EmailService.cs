

using Freeqy_APIs.Configrautions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Freeqy_APIs.Services;

/// <summary>
/// Mock email service that logs to console instead of sending real emails.
/// Replace with actual email service (SMTP/SendGrid) when ready.
/// </summary>
public class EmailService(ILogger<EmailService> logger, IOptions<MailConfig> mailConfig) : IEmailSender
{
    private readonly MailConfig  _mailConfig =  mailConfig.Value;
    private readonly ILogger<EmailService> _logger = logger;
//     public Task<bool> SendPasswordResetEmailAsync(
//         string toEmail, 
//         string resetToken, 
//         DateTime expiresAt, 
//         CancellationToken cancellationToken = default)
//     {
//         // Mock email sending by logging to console
//         var emailContent = $"""
//             ===============================================
//             PASSWORD RESET EMAIL (MOCK)
//             ===============================================
//             To: {toEmail}
//             Subject: Reset Your Password
//             
//             Hello,
//             
//             You requested to reset your password. Use the following token to reset your password:
//             
//             Token: {resetToken}
//             
//             This token will expire at: {expiresAt:yyyy-MM-dd HH:mm:ss} UTC
//             
//             If you didn't request this, please ignore this email.
//             
//             ===============================================
//             """;
//
//         _logger.LogInformation(emailContent);
//         Console.WriteLine(emailContent);
//
//         return Task.FromResult(true);
//     }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.Sender = MailboxAddress.Parse(_mailConfig.Mail);
        message.Subject = subject;
        message.To.Add(MailboxAddress.Parse(email));
        
        var builder = new BodyBuilder {HtmlBody = htmlMessage};
        message.Body = builder.ToMessageBody();
        
        using var smtp = new SmtpClient();
        
        smtp.Connect(_mailConfig.Host, _mailConfig.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailConfig.Mail, _mailConfig.Password);
        await smtp.SendAsync(message);
        _logger.LogInformation("Email sent to {}", email);
        smtp.Disconnect(true);
    }
}
