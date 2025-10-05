using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ChatGPT.API.Features.CreateConversation.Commands;

namespace ChatGPT.API.Features.CreateConversation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CreateConversationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateConversationController> _logger;

    public CreateConversationController(IMediator mediator, ILogger<CreateConversationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreateConversationResponse>> CreateConversation([FromBody] CreateConversationRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new CreateConversationCommand(userId, request.Title);
            var result = await _mediator.Send(command);
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return StatusCode(500, "Internal server error");
        }
    }
}

public record CreateConversationRequest(
    string Title = "New Conversation"
);


