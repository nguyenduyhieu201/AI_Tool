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
    public record LoginWithFacebookCommand(
        string Token) : ICommand<LoginResponseDto>;

    public class LoginWithFacebookCommandValidator : AbstractValidator<LoginWithFacebookCommand>
    {
        public LoginWithFacebookCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Facebook token is required");
        }
    }

    public class LoginWithFacebookCommandHandler : ICommandHandler<LoginWithFacebookCommand, LoginResponseDto>
    {
        private readonly IAuthenticationFactory _authFactory;
        private readonly ILogger<LoginWithFacebookCommandHandler> _logger;

        public LoginWithFacebookCommandHandler(
            IAuthenticationFactory authFactory,
            ILogger<LoginWithFacebookCommandHandler> logger)
        {
            _authFactory = authFactory ?? throw new ArgumentNullException(nameof(authFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LoginResponseDto> Handle(LoginWithFacebookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var strategy = _authFactory.CreateStrategy("Facebook");
                var result = await strategy.AuthenticateAsync(request.Token);

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.Message ?? "Failed to authenticate with Facebook");
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
                _logger.LogError(ex, "Error during Facebook authentication");
                throw;
            }
        }
    }
} 