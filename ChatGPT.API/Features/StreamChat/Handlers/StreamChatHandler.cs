using MediatR;
using ChatGPT.API.Features.StreamChat.Commands;
using ChatGPT.API.Shared.Services;
using ChatGPT.API.Shared.Models;

namespace ChatGPT.API.Features.StreamChat.Handlers;

public class StreamChatHandler : IRequestHandler<StreamChatCommand, IAsyncEnumerable<string>>
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly IChatGptService _chatGptService;
    private readonly IRedisService _redisService;
    private readonly ILogger<StreamChatHandler> _logger;

    public StreamChatHandler(
        IUserServiceClient userServiceClient,
        IChatGptService chatGptService,
        IRedisService redisService,
        ILogger<StreamChatHandler> logger)
    {
        _userServiceClient = userServiceClient;
        _chatGptService = chatGptService;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<IAsyncEnumerable<string>> Handle(StreamChatCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate user
            var user = await _userServiceClient.GetUserByIdAsync(request.UserId, "token");
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // 2. Get or create conversation
            var conversation = await GetOrCreateConversationAsync(request.UserId, request.ConversationId);

            // 3. Add user message
            var userMessage = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversation.Id,
                Content = request.Message,
                Role = "user",
                Timestamp = DateTime.UtcNow
            };
            await _redisService.StoreMessageAsync(userMessage);

            // 4. Stream ChatGPT response
            var streamResponse = _chatGptService.StreamResponseAsync(
                request.Message, 
                conversation.Messages,
                user,
                cancellationToken);

            return streamResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming chat for user {UserId}", request.UserId);
            throw;
        }
    }

    private async Task<Conversation> GetOrCreateConversationAsync(string userId, string? conversationId)
    {
        if (!string.IsNullOrEmpty(conversationId))
        {
            var existing = await _redisService.GetConversationAsync(conversationId);
            if (existing != null && existing.UserId == userId)
            {
                return existing;
            }
        }

        var conversation = new Conversation
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Title = "New Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages = new List<ChatMessage>()
        };

        await _redisService.StoreConversationAsync(conversation);
        return conversation;
    }
}


