using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlock.CQRS;
using FluentValidation;
using MediatR;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Domain.Models;

namespace Users.Application.Users.Commands.RequestPasswordReset
{
    public record RequestPasswordResetCommand(
        string Email,
        string IpAddress) : ICommand<bool>;

    public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
    {
        public RequestPasswordResetCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.IpAddress)
                .NotEmpty()
                .MaximumLength(45); // IPv6 max length
        }
    }

    public class RequestPasswordResetCommandHandler : ICommandHandler<RequestPasswordResetCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IEmailService _emailService;

        public RequestPasswordResetCommandHandler(
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            ITokenGenerator tokenGenerator,
            IEmailService emailService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordResetTokenRepository = passwordResetTokenRepository ?? throw new ArgumentNullException(nameof(passwordResetTokenRepository));
            _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<bool> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                // Return true even if user doesn't exist for security reasons
                return true;
            }

            // Revoke any existing valid tokens for this user
            await _passwordResetTokenRepository.RevokeValidTokensForUserAsync(user.Id, cancellationToken);

            // Generate new reset token
            var token = _tokenGenerator.GeneratePasswordResetToken();
            var expiresAt = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

            var passwordResetToken = PasswordResetToken.Create(user.Id, token, expiresAt, request.IpAddress);
            await _passwordResetTokenRepository.AddAsync(passwordResetToken, cancellationToken);

            // Send email with reset link
            var resetLink = $"http://localhost:5012/reset-password?token={token}";
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName, resetLink);

            return true;
        }
    }
}

