namespace Users.Application.Contracts.Security
{
    public interface ITokenGenerator
    {
        string GeneratePasswordResetToken();
        string GenerateEmailVerificationToken();
    }
}

