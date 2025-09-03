using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Users.Application.Users.Queries.GetUserDetails;

namespace Users.API.Endpoints
{
    public class GetUserEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/users/{id}", async (Guid id, ISender sender) =>
            {
                var user = await sender.Send(new GetUserDetailsQuery(id));
                if (user == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(user);
            });

            app.MapGet("/api/users/me", [Authorize] async (ClaimsPrincipal userPrincipal, ISender sender) =>
            {
                var sub = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? userPrincipal.FindFirst("sub")?.Value
                          ?? userPrincipal.FindFirst("uid")?.Value;
                if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
                {
                    return Results.Unauthorized();
                }

                var user = await sender.Send(new GetUserDetailsQuery(userId));
                if (user == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(user);
            })
            .WithName("GetCurrentUser");
        }
    }
}
