using Microsoft.AspNetCore.Identity;
using Resend;
using SuperInvestor.Data;

namespace SuperInvestor.Services;

public class ResendEmailSender(IResend resend, IConfiguration configuration) : IEmailSender<ApplicationUser>
{
    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var sender = configuration["Resend:SenderEmail"];
        var emailTemplate = @$"
            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #F0F4F8; border-radius: 10px;'>
                <h1 style='color: #1A3A5A; text-align: center;'>Welcome to Super Investor!</h1>
                <p style='font-size: 16px;'>Dear {user.Name},</p>
                <p style='font-size: 16px;'>You're about to unlock the power of intelligent investing. Super Investor is your gateway to professional-grade SEC filings research. Activate your account now to access:</p>
                <ul style='font-size: 16px;'>
                    <li><strong>Smart Note-Taking:</strong> Capture insights directly on filings, revolutionizing your research process.</li>
                    <li><strong>Lightning-Fast Lookup:</strong> Find the information you need in seconds, not hours.</li>
                    <li><strong>Collaborative Research:</strong> Share your findings and benefit from collective intelligence.</li>
                    <li><strong>Real-Time Updates:</strong> Stay ahead with instant notifications on new filings.</li>
                </ul>
                <div style='text-align: center; margin-top: 30px;'>
                    <a href='{confirmationLink}' style='display: inline-block; padding: 14px 30px; font-size: 18px; color: #fff; background-color: #4CAF50; text-decoration: none; border-radius: 5px; font-weight: bold;'>Activate Your Super Investor Account</a>
                </div>
                <p style='font-size: 14px; text-align: center; margin-top: 30px; color: #666;'>If you have any questions, please contact our support team at <a href='mailto:{sender}' style='color: #4A90E2;'>{sender}</a>.</p>
                <div style='text-align: center; margin-top: 20px;'>
                    <img src='' alt='Super Investor Logo' style='max-width: 150px;'>
                </div>
            </div>";

        var message = new EmailMessage
        {
            From = sender,
            To = email,
            Subject = "Welcome to Super Investor - Activate Your Account",
            HtmlBody = emailTemplate
        };

        await resend.EmailSendAsync(message);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var sender = configuration["Resend:SenderEmail"];
        var emailTemplate = @$"
            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #F0F4F8; border-radius: 10px;'>
                <h1 style='color: #1A3A5A; text-align: center;'>Password Reset Request</h1>
                <p style='font-size: 16px;'>Hello {user.Name},</p>
                <p style='font-size: 16px;'>We received a request to reset your Super Investor password. Security is our top priority, and we're here to help you regain access to your valuable research tools.</p>
                <div style='background-color: #E0E7FF; padding: 15px; border-radius: 5px; text-align: center; margin: 30px 0;'>
                    <p style='font-size: 18px; font-weight: bold; margin: 0;'>Your password reset code is:</p>
                    <p style='font-size: 24px; font-weight: bold; color: #1A3A5A; margin: 10px 0;'>{resetCode}</p>
                </div>
                <p style='font-size: 16px;'>Enter this code on the password reset page to create a new password. This code will expire in 15 minutes for your security.</p>
                <p style='font-size: 16px;'>If you didn't request this reset, please ignore this email or contact us immediately if you have concerns about your account security.</p>
                <p style='font-size: 14px; text-align: center; margin-top: 30px; color: #666;'>For any questions or assistance, please reach out to our dedicated support team at <a href='mailto:{sender}' style='color: #4A90E2;'>{sender}</a>.</p>
                <div style='text-align: center; margin-top: 20px;'>
                    <img src='' alt='Super Investor Logo' style='max-width: 150px;'>
                </div>
            </div>";

        var message = new EmailMessage
        {
            From = sender,
            To = email,
            Subject = "Super Investor - Password Reset Code",
            HtmlBody = emailTemplate
        };

        await resend.EmailSendAsync(message);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var sender = configuration["Resend:SenderEmail"];
        var emailTemplate = @$"
            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #F0F4F8; border-radius: 10px;'>
                <h1 style='color: #1A3A5A; text-align: center;'>Reset Your Super Investor Password</h1>
                <p style='font-size: 16px;'>Hello {user.Name},</p>
                <p style='font-size: 16px;'>We've received a request to reset your Super Investor password. Your investment insights are valuable, and we want to ensure you regain access to your account securely.</p>
                <div style='text-align: center; margin-top: 30px;'>
                    <a href='{resetLink}' style='display: inline-block; padding: 14px 30px; font-size: 18px; color: #fff; background-color: #4CAF50; text-decoration: none; border-radius: 5px; font-weight: bold;'>Reset Your Password</a>
                </div>
                <p style='font-size: 16px; margin-top: 30px;'>This link will expire in 15 minutes for your security. If you didn't request this reset, please disregard this email or contact us if you have any concerns about your account's security.</p>
                <p style='font-size: 16px;'>Remember, Super Investor is here to empower your investment decisions with cutting-edge research tools. We look forward to seeing you back on the platform!</p>
                <p style='font-size: 14px; text-align: center; margin-top: 30px; color: #666;'>If you need any assistance, our expert support team is ready to help at <a href='mailto:{sender}' style='color: #4A90E2;'>{sender}</a>.</p>
                <div style='text-align: center; margin-top: 20px;'>
                    <img src='' alt='Super Investor Logo' style='max-width: 150px;'>
                </div>
            </div>";

        var message = new EmailMessage
        {
            From = sender,
            To = email,
            Subject = "Super Investor - Reset Your Password",
            HtmlBody = emailTemplate
        };

        await resend.EmailSendAsync(message);
    }
}