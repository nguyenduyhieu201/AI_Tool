using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlock.CQRS;
using FluentValidation;
using MediatR;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;

namespace Users.Application.Users.Commands.ResetPassword
{
    public record ResetPasswordCommand(
        string Token,
        string NewPassword,
        string IpAddress) : ICommand<bool>;

    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?])[A-Za-z\d!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number and one special character");

            RuleFor(x => x.IpAddress)
                .NotEmpty()
                .MaximumLength(45);
        }
    }

    public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, bool>
    {
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordCommandHandler(
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
        {
            _passwordResetTokenRepository = passwordResetTokenRepository ?? throw new ArgumentNullException(nameof(passwordResetTokenRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(request.Token, cancellationToken);
            if (resetToken == null || !resetToken.IsValid)
            {
                throw new InvalidOperationException("Invalid or expired reset token.");
            }

            var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Hash new password
            var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

            // Update user password
            user.UpdatePassword(newPasswordHash);
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Mark reset token as used
            resetToken.MarkAsUsed();
            await _passwordResetTokenRepository.UpdateAsync(resetToken, cancellationToken);

            // Revoke all refresh tokens for security
            user.RevokeAllRefreshTokens(request.IpAddress, "Password changed");

            return true;
        }
    }
}

