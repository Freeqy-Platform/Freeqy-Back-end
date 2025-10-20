namespace Freeqy_APIs.Services;

/// <summary>
/// Mock email service that logs to console instead of sending real emails.
/// Replace with actual email service (SMTP/SendGrid) when ready.
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendPasswordResetEmailAsync(
        string toEmail, 
        string resetToken, 
        DateTime expiresAt, 
        CancellationToken cancellationToken = default)
    {
        // Mock email sending by logging to console
        var emailContent = $"""
            ===============================================
            PASSWORD RESET EMAIL (MOCK)
            ===============================================
            To: {toEmail}
            Subject: Reset Your Password
            
            Hello,
            
            You requested to reset your password. Use the following token to reset your password:
            
            Token: {resetToken}
            
            This token will expire at: {expiresAt:yyyy-MM-dd HH:mm:ss} UTC
            
            If you didn't request this, please ignore this email.
            
            ===============================================
            """;

        _logger.LogInformation(emailContent);
        Console.WriteLine(emailContent);

        return Task.FromResult(true);
    }
}
