using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlock.CQRS;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Users.Application.Authentication.Factory;
using Users.Application.Contracts.Authentication.Strategies;
using Users.Domain.Models;

namespace Users.Application.Users.Commands.Login
{
    public record LoginWithGoogleCommand(
        string Token) : ICommand<LoginResponseDto>;

    public class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
    {
        public LoginWithGoogleCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Google token is required");
        }
    }

    public class LoginWithGoogleCommandHandler : ICommandHandler<LoginWithGoogleCommand, LoginResponseDto>
    {
        private readonly IAuthenticationFactory _authFactory;
        private readonly ILogger<LoginWithGoogleCommandHandler> _logger;

        public LoginWithGoogleCommandHandler(
            IAuthenticationFactory authFactory,
            ILogger<LoginWithGoogleCommandHandler> logger)
        {
            _authFactory = authFactory ?? throw new ArgumentNullException(nameof(authFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LoginResponseDto> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var strategy = _authFactory.CreateStrategy("Google");
                var result = await strategy.AuthenticateAsync(request.Token);

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.Message ?? "Failed to authenticate with Google");
                }

                return new LoginResponseDto(
                    result.User.Id,
                    result.User.FirstName,
                    result.User.LastName,
                    result.User.Email,
                    result.AccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google authentication");
                throw;
            }
        }
    }
}
