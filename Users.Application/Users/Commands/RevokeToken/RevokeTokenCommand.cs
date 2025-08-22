using System;
using BuildingBlock.CQRS;
using MediatR;

namespace Users.Application.Users.Commands.RevokeToken
{
    public class RevokeTokenCommand : ICommand<bool>
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }
}