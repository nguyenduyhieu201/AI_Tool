using System;
using BuildingBlock.CQRS;
using MediatR;
using Users.Application.Contracts.Security;

namespace Users.Application.Users.Commands.RefreshToken
{
    public class RefreshTokenCommand : ICommand<RefreshTokenResponse>
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }

    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}