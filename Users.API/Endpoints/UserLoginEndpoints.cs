using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.Domain;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Users.Domain.Models;
using MediatR;
using Users.Application.Users.Commands.Login;
using Carter;

namespace Users.API.Endpoints;

public class UserLoginEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IConfiguration configuration,
            ISender sender) =>
        {
            var user = await sender.Send(new LoginCommand(request.Username, request.Password));
            if (user == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(user.Token);
        })
        .WithName("LoginUser");
    }



} 