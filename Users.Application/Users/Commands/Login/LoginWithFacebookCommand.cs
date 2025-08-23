using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlock.CQRS;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Users.Application.Authentication.Factory;
using Users.Application.Contracts.Authentication.Strategies;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Domain.Models;
using RefToken = Users.Domain.Models.RefreshToken;


namespace Users.Application.Users.Commands.Login
{
    public record LoginWithFacebookCommand(
        string Token,
        string IpAddress) : ICommand<LoginResponseDto>;

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
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;


        public LoginWithFacebookCommandHandler(
            IAuthenticationFactory authFactory,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtService jwtService,
            ILogger<LoginWithFacebookCommandHandler> logger)
        {
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _authFactory = authFactory ?? throw new ArgumentNullException(nameof(authFactory));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
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

                // Generate refresh token
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Create and save refresh token
                var refreshTokenEntity = RefToken.Create(
                    refreshToken,
                    DateTime.UtcNow.AddDays(7),
                    result.User.Id,
                    request.IpAddress
                );

                await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

                return new LoginResponseDto(
                    result.User.Id,
                    result.User.FirstName,
                    result.User.LastName,
                    result.User.Email,
                    result.AccessToken,
                    refreshToken,
                    refreshTokenEntity.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Facebook authentication");
                throw;
            }
        }
    }
} 