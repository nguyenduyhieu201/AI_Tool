using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.Application.Users.Commands.UpdateUser;

namespace Users.API.Endpoints
{
    public class UpdateUserEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/users/{id}", async (Guid id, [FromBody] UpdateUserRequest request, ISender sender) =>
            {
                var command = new UpdateUserCommand(id, request.FirstName, request.LastName, request.Email);
                var result = await sender.Send(command);
                if (result.IsSuccess)
                {
                    return Results.Ok(result.Value);
                }
                return Results.BadRequest(result.Error);
            }); 
        }
    }
}
