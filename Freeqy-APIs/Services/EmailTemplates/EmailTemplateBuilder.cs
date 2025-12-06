namespace Freeqy_APIs.Services.EmailTemplates;

public static class EmailTemplateBuilder
{
	public static string BuildEmailConfirmationTemplate(string confirmationLink)
	{
		return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4F46E5; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4F46E5; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .warning {{ background-color: #FEF3C7; border-left: 4px solid #F59E0B; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Email Confirmation Required</h1>
        </div>
        <div class='content'>
            <h2>Hello!</h2>
            <p>You recently changed your email address on <strong>Freeqy Platform</strong>.</p>
            <p>Please confirm your new email address by clicking the button below:</p>
            
            <div style='text-align: center;'>
                <a href='{confirmationLink}' class='button'>Confirm Email Address</a>
            </div>
            
            <p>Or copy and paste this link into your browser:</p>
            <p style='background-color: #e9ecef; padding: 10px; word-break: break-all; font-size: 12px;'>{confirmationLink}</p>
            
            <div class='warning'>
                <strong>?? Important:</strong>
                <ul>
                    <li>This link will expire in <strong>24 hours</strong></li>
                    <li>If you didn't request this change, please contact support immediately</li>
                    <li>Your account security is important to us</li>
                </ul>
            </div>
        </div>
        <div class='footer'>
            <p>&copy; 2024 Freeqy Platform. All rights reserved.</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
	}

	public static string BuildEmailChangeNotificationTemplate(string oldEmail, string newEmail)
	{
		return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #EF4444; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .alert {{ background-color: #FEE2E2; border-left: 4px solid #EF4444; padding: 15px; margin: 20px 0; }}
        .email-change {{ background-color: white; padding: 20px; margin: 20px 0; border-radius: 5px; border: 1px solid #ddd; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Security Alert</h1>
        </div>
        <div class='content'>
            <h2>Email Address Changed</h2>
            <p>This is a security notification to inform you that your email address has been changed.</p>
            
            <div class='email-change'>
                <p><strong>Previous Email:</strong></p>
                <p style='font-size: 16px; color: #666;'>{oldEmail}</p>
                <p style='text-align: center; margin: 15px 0;'>??</p>
                <p><strong>New Email:</strong></p>
                <p style='font-size: 16px; color: #4F46E5;'>{newEmail}</p>
            </div>
            
            <div class='alert'>
                <strong>?? If this wasn't you:</strong>
                <p>Please contact our support team <strong>immediately</strong> at <a href='mailto:support@freeqy.com'>support@freeqy.com</a></p>
                <p>Your account security may be compromised.</p>
            </div>
            
            <p><strong>Security Tips:</strong></p>
            <ul>
                <li>Never share your password with anyone</li>
                <li>Enable two-factor authentication for extra security</li>
                <li>Use a strong, unique password</li>
            </ul>
        </div>
        <div class='footer'>
            <p>&copy; 2024 Freeqy Platform. All rights reserved.</p>
            <p>This is an automated security notification.</p>
        </div>
    </div>
</body>
</html>";
	}

	public static string BuildPasswordChangedNotificationTemplate(string changeTime, string ipAddress, string userAgent)
	{
		return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10B981; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .alert {{ background-color: #FEE2E2; border-left: 4px solid #EF4444; padding: 15px; margin: 20px 0; }}
        .info-box {{ background-color: white; padding: 20px; margin: 20px 0; border-radius: 5px; border: 1px solid #ddd; }}
        .info-item {{ margin: 10px 0; padding: 8px; background-color: #f9fafb; border-radius: 3px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .success-badge {{ background-color: #D1FAE5; color: #065F46; padding: 5px 15px; border-radius: 20px; display: inline-block; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>? Password Successfully Changed</h1>
        </div>
        <div class='content'>
            <div style='text-align: center; margin: 20px 0;'>
                <span class='success-badge'>? Confirmed</span>
            </div>
            
            <h2>Your Password Has Been Updated</h2>
            <p>This is a security notification to confirm that your password was successfully changed on <strong>Freeqy Platform</strong>.</p>
            
            <div class='info-box'>
                <h3>Change Details:</h3>
                <div class='info-item'>
                    <strong>?? Date & Time:</strong><br/>
                    {changeTime}
                </div>
                <div class='info-item'>
                    <strong>?? IP Address:</strong><br/>
                    {ipAddress}
                </div>
                <div class='info-item'>
                    <strong>?? Device:</strong><br/>
                    {userAgent}
                </div>
            </div>
            
            <div class='alert'>
                <strong>?? If this wasn't you:</strong>
                <p>Your account may have been compromised. Please take the following actions <strong>immediately</strong>:</p>
                <ol>
                    <li>Reset your password using the 'Forgot Password' option</li>
                    <li>Review your recent account activity</li>
                    <li>Contact our support team at <a href='mailto:support@freeqy.com'>support@freeqy.com</a></li>
                    <li>Enable two-factor authentication for extra security</li>
                </ol>
            </div>
            
            <div style='background-color: #EFF6FF; padding: 20px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #3B82F6;'>
                <h3 style='color: #1E40AF; margin-top: 0;'>??? Security Best Practices:</h3>
                <ul>
                    <li>Use a unique password for each online account</li>
                    <li>Enable two-factor authentication (2FA)</li>
                    <li>Never share your password with anyone</li>
                    <li>Use a password manager to store complex passwords</li>
                    <li>Change your password regularly (every 3-6 months)</li>
                    <li>Avoid using personal information in passwords</li>
                </ul>
            </div>
            
            <p><strong>Note:</strong> You have been automatically logged out from all devices for security reasons. Please log in again with your new password.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 Freeqy Platform. All rights reserved.</p>
            <p>This is an automated security notification.</p>
            <p>If you have any questions, contact us at <a href='mailto:support@freeqy.com'>support@freeqy.com</a></p>
        </div>
    </div>
</body>
</html>";
	}

	public static string BuildWelcomeEmailTemplate(string userName, string confirmationLink)
	{
		return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4F46E5; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4F46E5; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .features {{ background-color: white; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .feature-item {{ margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Welcome to Freeqy Platform!</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName}!</h2>
            <p>Thank you for joining <strong>Freeqy Platform</strong> - the best place to find team members and collaborate on amazing projects!</p>
            
            <p>To get started, please confirm your email address by clicking the button below:</p>
            
            <div style='text-align: center;'>
                <a href='{confirmationLink}' class='button'>Confirm Email Address</a>
            </div>
            
            <div class='features'>
                <h3>What you can do on Freeqy:</h3>
                <div class='feature-item'>? Create and manage projects</div>
                <div class='feature-item'>? Find skilled team members</div>
                <div class='feature-item'>? Showcase your portfolio</div>
                <div class='feature-item'>? Collaborate with developers worldwide</div>
            </div>
            
            <p>If you didn't create this account, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 Freeqy Platform. All rights reserved.</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
	}

	public static string BuildPasswordResetTemplate(string resetLink)
	{
		return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #F59E0B; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #F59E0B; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .warning {{ background-color: #FEE2E2; border-left: 4px solid #EF4444; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Password Reset Request</h1>
        </div>
        <div class='content'>
            <h2>Reset Your Password</h2>
            <p>We received a request to reset your password on <strong>Freeqy Platform</strong>.</p>
            <p>Click the button below to reset your password:</p>
            
            <div style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </div>
            
            <p>Or copy and paste this link into your browser:</p>
            <p style='background-color: #e9ecef; padding: 10px; word-break: break-all; font-size: 12px;'>{resetLink}</p>
            
            <div class='warning'>
                <strong>?? Security Notice:</strong>
                <ul>
                    <li>This link will expire in <strong>1 hour</strong></li>
                    <li>If you didn't request this, please ignore this email</li>
                    <li>Your password will remain unchanged</li>
                </ul>
            </div>
        </div>
        <div class='footer'>
            <p>&copy; 2024 Freeqy Platform. All rights reserved.</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
	}
}
