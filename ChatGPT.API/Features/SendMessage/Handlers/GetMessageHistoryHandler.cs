using MediatR;
using ChatGPT.API.Features.SendMessage.Queries;
using ChatGPT.API.Shared.Services;

namespace ChatGPT.API.Features.SendMessage.Handlers;

public class GetMessageHistoryHandler : IRequestHandler<GetMessageHistoryQuery, GetMessageHistoryResponse>
{
    private readonly IRedisService _redisService;
    private readonly ILogger<GetMessageHistoryHandler> _logger;

    public GetMessageHistoryHandler(IRedisService redisService, ILogger<GetMessageHistoryHandler> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<GetMessageHistoryResponse> Handle(GetMessageHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var conversation = await _redisService.GetConversationAsync(request.ConversationId);
            if (conversation == null || conversation.UserId != request.UserId)
            {
                return new GetMessageHistoryResponse(new List<MessageDto>(), 0);
            }

            var messages = conversation.Messages
                .TakeLast(request.Limit)
                .Select(m => new MessageDto(
                    m.Id,
                    m.Content,
                    m.Role,
                    m.Timestamp,
                    m.TokenCount
                ))
                .ToList();

            return new GetMessageHistoryResponse(messages, messages.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message history for conversation {ConversationId}", request.ConversationId);
            throw;
        }
    }
}


