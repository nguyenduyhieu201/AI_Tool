using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.Application.Users.Commands.CreateUser;
using Users.Application.Users.Queries.GetUserDetails;

namespace Users.API.Endpoints
{
    public class RegisterUserEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users/register", async ([FromBody] RegisterUserRequest request, ISender sender) =>
            {
                var id = await sender.Send(new CreateUserCommand(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Password,
                    request.PhoneNumber));

                var user = await sender.Send(new GetUserDetailsQuery(id));
                return Results.Created($"/api/users/{id}", user);
            })
            .WithName("RegisterUser");
        }
    }
}



