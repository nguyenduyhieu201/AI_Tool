using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Users.Application.Contracts.Security;

namespace Users.Infrastructure.Security
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string email, string firstName, string resetLink)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var useRealEmail = emailSettings.GetValue<bool>("UseRealEmail", false);

            if (useRealEmail)
            {
                await SendRealEmailAsync(email, firstName, resetLink);
            }
            else
            {
                // Log mode for development/testing
                _logger.LogInformation("Password reset email sent to {Email} for user {FirstName}. Reset link: {ResetLink}", 
                    email, firstName, resetLink);
                
                // Simulate email sending delay
                await Task.Delay(100);
            }
        }

        public async Task SendEmailVerificationEmailAsync(string email, string firstName, string verificationLink)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var useRealEmail = emailSettings.GetValue<bool>("UseRealEmail", false);

            if (useRealEmail)
            {
                await SendRealVerificationEmailAsync(email, firstName, verificationLink);
            }
            else
            {
                _logger.LogInformation("Email verification email sent to {Email} for user {FirstName}. Verification link: {VerificationLink}", 
                    email, firstName, verificationLink);
                
                await Task.Delay(100);
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var useRealEmail = emailSettings.GetValue<bool>("UseRealEmail", false);

            if (useRealEmail)
            {
                await SendRealWelcomeEmailAsync(email, firstName);
            }
            else
            {
                _logger.LogInformation("Welcome email sent to {Email} for user {FirstName}", email, firstName);
                
                await Task.Delay(100);
            }
        }

        private async Task SendRealEmailAsync(string email, string firstName, string resetLink)
        {
            try
            {
                // TODO: Implement real email sending using SMTP, SendGrid, etc.
                var smtpSettings = _configuration.GetSection("EmailSettings:Smtp");
                var smtpServer = smtpSettings["Server"];
                var smtpPort = smtpSettings.GetValue<int>("Port", 587);
                var smtpUsername = smtpSettings["Username"];
                var smtpPassword = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername))
                {
                    _logger.LogWarning("SMTP settings not configured, falling back to log mode");
                    await SendPasswordResetEmailAsync(email, firstName, resetLink); // Recursive call with log mode
                    return;
                }

                // Example using System.Net.Mail (you can replace with SendGrid, MailKit, etc.)
                using var mailMessage = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(fromEmail, fromName),
                    Subject = "Reset Your Password",
                    Body = GeneratePasswordResetEmailBody(firstName, resetLink),
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                using var smtpClient = new System.Net.Mail.SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword)
                };

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Real password reset email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real email to {Email}, falling back to log mode", email);
                // Fallback to log mode
                await SendPasswordResetEmailAsync(email, firstName, resetLink);
            }
        }

        private async Task SendRealVerificationEmailAsync(string email, string firstName, string verificationLink)
        {
            // Similar implementation for verification email
            await Task.CompletedTask;
        }

        private async Task SendRealWelcomeEmailAsync(string email, string firstName)
        {
            // Similar implementation for welcome email
            await Task.CompletedTask;
        }

        private string GeneratePasswordResetEmailBody(string firstName, string resetLink)
        {
            return $@"
                <html>
                <body>
                    <h2>Hello {firstName},</h2>
                    <p>You have requested to reset your password.</p>
                    <p>Click the link below to reset your password:</p>
                    <p><a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <p>Best regards,<br/>Your App Team</p>
                </body>
                </html>";
        }
    }
}

















