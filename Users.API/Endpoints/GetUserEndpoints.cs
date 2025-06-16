using Carter;
using MediatR;
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
        }
    }
}
