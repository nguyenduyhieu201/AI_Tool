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
                var command = new UpdateUserCommand(id, request.FirstName, request.LastName, request.Email, request.PhoneNumber);
                var result = await sender.Send(command);
                if(result == Guid.Empty)
                {
                    throw new Exception("Failed to update userr");
                }
                return Results.Ok(result);
            }); 
        }
    }
}
