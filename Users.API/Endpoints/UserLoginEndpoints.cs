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
using Users.Application.Users.Commands.RefreshToken;
using Carter;

namespace Users.API.Endpoints;

public class UserLoginEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user")
            .WithTags("Authentication");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IConfiguration configuration,
            ISender sender) =>
        {
            var user = await sender.Send(new LoginCommand(request.Username, request.Password, request.IpAddress));
            if (user == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(user);
        })
        .WithName("LoginUser");

        // Thêm endpoint refresh token theo pattern có sẵn
        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenRequest request,
            ISender sender) =>
        {
            try
            {
                var result = await sender.Send(new RefreshTokenCommand(
                    request.AccessToken, 
                    request.RefreshToken, 
                    request.IpAddress));
                
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error refreshing token: {ex.Message}");
            }
        })
        .WithName("RefreshToken")
        .Produces<Users.Application.Users.Commands.RefreshToken.RefreshTokenResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
} 