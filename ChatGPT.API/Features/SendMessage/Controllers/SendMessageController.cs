using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ChatGPT.API.Features.SendMessage.Commands;
using ChatGPT.API.Features.SendMessage.Queries;

namespace ChatGPT.API.Features.SendMessage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SendMessageController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SendMessageController> _logger;

    public SendMessageController(IMediator mediator, ILogger<SendMessageController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<SendMessageResponse>> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new SendMessageCommand(userId, request.Message, request.ConversationId);
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
            _logger.LogError(ex, "Error in SendMessage");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("history/{conversationId}")]
    public async Task<ActionResult<GetMessageHistoryResponse>> GetMessageHistory(
        string conversationId, 
        [FromQuery] int limit = 50)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var query = new GetMessageHistoryQuery(userId, conversationId, limit);
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message history");
            return StatusCode(500, "Internal server error");
        }
    }
}

public record SendMessageRequest(
    string Message,
    string? ConversationId = null
);


