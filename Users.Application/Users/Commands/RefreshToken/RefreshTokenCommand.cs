using System;
using BuildingBlock.CQRS;
using MediatR;
using Users.Application.Contracts.Security;

namespace Users.Application.Users.Commands.RefreshToken
{
    public record RefreshTokenCommand(
        string AccessToken,
        string RefreshToken,
        string IpAddress) : ICommand<RefreshTokenResponse>;

    public record RefreshTokenResponse(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAt);
}