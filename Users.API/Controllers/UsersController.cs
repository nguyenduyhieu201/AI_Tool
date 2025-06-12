using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Users.Commands.CreateUser;
using Users.Application.Users.Commands.Login;
using Users.Application.Users.Queries.GetUserDetails;

namespace Users.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateUserCommand command, CancellationToken cancellationToken)
        {
            var userId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = userId }, userId);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginCommand command, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var user = await _mediator.Send(new GetUserDetailsQuery(id), cancellationToken);
            
            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
} 