using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.Application.Users.Commands.RequestPasswordReset;
using Users.Application.Users.Commands.ResetPassword;

namespace Users.API.Endpoints
{
    public class PasswordResetEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users/forgot-password", async (
                [FromBody] RequestPasswordResetRequest request,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                
                var result = await sender.Send(new RequestPasswordResetCommand(
                    request.Email,
                    ipAddress));

                return Results.Ok(new { Message = "If the email exists, a password reset link has been sent." });
            })
            .WithName("RequestPasswordReset");

            app.MapPost("/api/users/reset-password", async (
                [FromBody] ResetPasswordRequest request,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                
                var result = await sender.Send(new ResetPasswordCommand(
                    request.Token,
                    request.NewPassword,
                    ipAddress));

                return Results.Ok(new { Message = "Password has been reset successfully." });
            })
            .WithName("ResetPassword");
        }
    }
}

