namespace EGreetings.Notification.Application.DTOs;

public static class EmailTemplates
{
    public static string WelcomeEmail(string fullName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; }}
        .header {{ color: #333; text-align: center; }}
        .content {{ color: #666; line-height: 1.6; margin: 20px 0; }}
        .footer {{ color: #999; font-size: 12px; text-align: center; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to E-Greetings!</h1>
        </div>
        <div class='content'>
            <p>Dear {fullName},</p>
            <p>Thank you for registering with E-Greetings. We're excited to have you on board!</p>
            <p>Start sending beautiful greetings to your loved ones today.</p>
            <p>Best regards,<br>The E-Greetings Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 E-Greetings. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GreetingNotification(string recipientName, string senderMessage, string viewUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; }}
        .header {{ color: #333; text-align: center; }}
        .content {{ color: #666; line-height: 1.6; margin: 20px 0; }}
        .message-box {{ background-color: #f9f9f9; padding: 15px; border-left: 4px solid #4CAF50; margin: 15px 0; }}
        .button {{ display: inline-block; background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; margin: 15px 0; }}
        .footer {{ color: #999; font-size: 12px; text-align: center; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>You've Received a Greeting!</h1>
        </div>
        <div class='content'>
            <p>Hi {recipientName},</p>
            <p>Someone special sent you a greeting message:</p>
            <div class='message-box'>
                <p>{senderMessage}</p>
            </div>
            <a href='{viewUrl}' class='button'>View Greeting</a>
            <p>Best regards,<br>The E-Greetings Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 E-Greetings. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public static string PaymentConfirmed(string fullName, decimal amount, DateTime expiredAt)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; }}
        .header {{ color: #333; text-align: center; }}
        .content {{ color: #666; line-height: 1.6; margin: 20px 0; }}
        .receipt {{ background-color: #f9f9f9; padding: 15px; border: 1px solid #ddd; border-radius: 4px; margin: 15px 0; }}
        .receipt-item {{ display: flex; justify-content: space-between; margin: 8px 0; }}
        .footer {{ color: #999; font-size: 12px; text-align: center; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Payment Confirmed</h1>
        </div>
        <div class='content'>
            <p>Dear {fullName},</p>
            <p>Your subscription payment has been successfully processed.</p>
            <div class='receipt'>
                <div class='receipt-item'>
                    <span>Amount Paid:</span>
                    <span><strong>${amount:F2}</strong></span>
                </div>
                <div class='receipt-item'>
                    <span>Subscription Valid Until:</span>
                    <span><strong>{expiredAt:yyyy-MM-dd}</strong></span>
                </div>
            </div>
            <p>Thank you for your subscription. Enjoy unlimited greetings!</p>
            <p>Best regards,<br>The E-Greetings Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 E-Greetings. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public static string PasswordReset(string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; }}
        .header {{ color: #333; text-align: center; }}
        .content {{ color: #666; line-height: 1.6; margin: 20px 0; }}
        .button {{ display: inline-block; background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; margin: 15px 0; }}
        .warning {{ background-color: #fff3cd; padding: 10px; border-radius: 4px; margin: 15px 0; color: #856404; }}
        .footer {{ color: #999; font-size: 12px; text-align: center; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hi,</p>
            <p>We received a request to reset your password. Click the button below to set a new password:</p>
            <a href='{resetUrl}' class='button'>Reset Password</a>
            <div class='warning'>
                <p><strong>Note:</strong> This link will expire in 24 hours.</p>
            </div>
            <p>If you didn't request this, please ignore this email.</p>
            <p>Best regards,<br>The E-Greetings Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 E-Greetings. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}
