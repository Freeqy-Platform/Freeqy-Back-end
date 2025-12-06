using Freeqy_APIs.Configrautions;
using Freeqy_APIs.Services.EmailTemplates;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Freeqy_APIs.Services;

/// <summary>
/// Email service for sending emails using SMTP.
/// </summary>
public class EmailService(ILogger<EmailService> logger, IOptions<MailConfig> mailConfig, IConfiguration configuration) : IEmailSender
{
    private readonly MailConfig  _mailConfig =  mailConfig.Value;
    private readonly ILogger<EmailService> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

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
        _logger.LogInformation("Email sent to {Email}", email);
        smtp.Disconnect(true);
    }

    public async Task SendEmailConfirmationAsync(string email, string token, string userId)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var encodedUserId = Uri.EscapeDataString(userId);
        
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
        var confirmationLink = $"{baseUrl}/api/users/confirm-email?userId={encodedUserId}&token={encodedToken}";
        
        var subject = "Confirm Your New Email Address";
        var htmlMessage = EmailTemplateBuilder.BuildEmailConfirmationTemplate(confirmationLink);

        await SendEmailAsync(email, subject, htmlMessage);
        _logger.LogInformation("Email confirmation sent to {Email} for user {UserId}", email, userId);
    }

    public async Task SendEmailChangeNotificationAsync(string oldEmail, string newEmail)
    {
        var subject = "?? Security Alert: Email Address Changed";
        var htmlMessage = EmailTemplateBuilder.BuildEmailChangeNotificationTemplate(oldEmail, newEmail);

        await SendEmailAsync(oldEmail, subject, htmlMessage);
        _logger.LogInformation("Email change notification sent to {OldEmail}", oldEmail);
    }

    public async Task SendPasswordChangedNotificationAsync(string email, string changeTime, string ipAddress, string userAgent)
    {
        var subject = "? Password Successfully Changed";
        var htmlMessage = EmailTemplateBuilder.BuildPasswordChangedNotificationTemplate(changeTime, ipAddress, userAgent);

        await SendEmailAsync(email, subject, htmlMessage);
        _logger.LogInformation("Password changed notification sent to {Email}", email);
    }

    public async Task SendWelcomeEmailAsync(string email, string userName, string token, string userId)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var encodedUserId = Uri.EscapeDataString(userId);
        
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
        var confirmationLink = $"{baseUrl}/api/auth/confirm-email?userId={encodedUserId}&token={encodedToken}";
        
        var subject = "?? Welcome to Freeqy Platform!";
        var htmlMessage = EmailTemplateBuilder.BuildWelcomeEmailTemplate(userName, confirmationLink);

        await SendEmailAsync(email, subject, htmlMessage);
        _logger.LogInformation("Welcome email sent to {Email}", email);
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        var encodedToken = Uri.EscapeDataString(resetToken);
        
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
        var resetLink = $"{baseUrl}/reset-password?token={encodedToken}";
        
        var subject = "?? Password Reset Request";
        var htmlMessage = EmailTemplateBuilder.BuildPasswordResetTemplate(resetLink);

        await SendEmailAsync(email, subject, htmlMessage);
        _logger.LogInformation("Password reset email sent to {Email}", email);
    }
}
