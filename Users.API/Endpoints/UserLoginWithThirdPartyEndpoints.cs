using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.Application.Users.Commands.Login;

namespace Users.API.Endpoints
{
    public class UserLoginWithThirdPartyEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("Authentication");

            group.MapPost("/login/google", async (
                [FromBody] ThirdPartyLoginRequest request,
                ISender sender) =>
            {
                var result = await sender.Send(new LoginWithGoogleCommand(request.Token));
                return Results.Ok(result);
            })
            .WithName("LoginWithGoogle")
            .Produces<LoginResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/login/facebook", async (
                [FromBody] ThirdPartyLoginRequest request,
                ISender sender) =>
            {
                var result = await sender.Send(new LoginWithFacebookCommand(request.Token));
                return Results.Ok(result);
            })
            .WithName("LoginWithFacebook")
            .Produces<LoginResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
