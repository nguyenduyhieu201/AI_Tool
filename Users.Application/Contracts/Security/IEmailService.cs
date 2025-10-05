namespace Users.Application.Contracts.Security
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string firstName, string resetLink);
        Task SendEmailVerificationEmailAsync(string email, string firstName, string verificationLink);
        Task SendWelcomeEmailAsync(string email, string firstName);
    }
}

