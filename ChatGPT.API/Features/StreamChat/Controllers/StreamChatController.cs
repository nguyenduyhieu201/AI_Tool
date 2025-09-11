using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ChatGPT.API.Features.StreamChat.Commands;

namespace ChatGPT.API.Features.StreamChat.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreamChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StreamChatController> _logger;

    public StreamChatController(IMediator mediator, ILogger<StreamChatController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("stream")]
    public async Task<IActionResult> StreamChat([FromBody] StreamChatRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new StreamChatCommand(userId, request.Message, request.ConversationId);
            var stream = await _mediator.Send(command);

            Response.Headers.Add("Content-Type", "text/plain");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            await foreach (var chunk in stream)
            {
                await Response.WriteAsync(chunk);
                await Response.Body.FlushAsync();
            }

            return new EmptyResult();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in StreamChat");
            return StatusCode(500, "Internal server error");
        }
    }
}

public record StreamChatRequest(
    string Message,
    string? ConversationId = null
);


