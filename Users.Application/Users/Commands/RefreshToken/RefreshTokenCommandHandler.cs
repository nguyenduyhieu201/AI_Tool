using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Domain.Models;
using RefToken = Users.Domain.Models.RefreshToken;

namespace Users.Application.Users.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtService _jwtService;

        public RefreshTokenCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IJwtService jwtService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Get principal from expired access token
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                throw new UnauthorizedAccessException("Invalid access token");
            }

            var userId = Guid.Parse(principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);

            // Get refresh token from database
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (refreshToken == null || refreshToken.UserId != userId || !refreshToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            // Revoke the current refresh token
            refreshToken.Revoke(request.IpAddress, "Replaced by new token");
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

            // Generate new tokens
            var newRefreshToken = RefToken.Create(
                _jwtService.GenerateRefreshToken(),
                DateTime.UtcNow.AddDays(7),
                userId,
                request.IpAddress
            );

            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

            // Generate new access token
            var newAccessToken = _jwtService.GenerateToken(refreshToken.User);

            // Return using record positional parameters
            return new RefreshTokenResponse(
                newAccessToken,
                newRefreshToken.Token,
                newRefreshToken.ExpiresAt
            );
        }
    }
}