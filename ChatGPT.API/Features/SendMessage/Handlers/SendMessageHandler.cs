using MediatR;
using ChatGPT.API.Features.SendMessage.Commands;
using ChatGPT.API.Shared.Services;
using ChatGPT.API.Shared.Models;

namespace ChatGPT.API.Features.SendMessage.Handlers;

public class SendMessageHandler : IRequestHandler<SendMessageCommand, SendMessageResponse>
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly IChatGptService _chatGptService;
    private readonly IRedisService _redisService;
    private readonly ILogger<SendMessageHandler> _logger;

    public SendMessageHandler(
        IUserServiceClient userServiceClient,
        IChatGptService chatGptService,
        IRedisService redisService,
        ILogger<SendMessageHandler> logger)
    {
        _userServiceClient = userServiceClient;
        _chatGptService = chatGptService;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<SendMessageResponse> Handle(SendMessageCommand request, CancellationToken cancellationToken)
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

            // 4. Generate ChatGPT response
            var chatGptResponse = await _chatGptService.GenerateResponseAsync(
                request.Message, 
                conversation.Messages,
                user);

            // 5. Add assistant message
            var assistantMessage = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversation.Id,
                Content = chatGptResponse,
                Role = "assistant",
                Timestamp = DateTime.UtcNow,
                TokenCount = CalculateTokenCount(chatGptResponse)
            };
            await _redisService.StoreMessageAsync(assistantMessage);

            // 6. Update conversation
            conversation.Messages.Add(userMessage);
            conversation.Messages.Add(assistantMessage);
            conversation.UpdatedAt = DateTime.UtcNow;
            await _redisService.UpdateConversationAsync(conversation);

            return new SendMessageResponse(
                chatGptResponse,
                conversation.Id,
                DateTime.UtcNow,
                assistantMessage.TokenCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message for user {UserId}", request.UserId);
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

    private int CalculateTokenCount(string text)
    {
        return text.Split(' ').Length;
    }
}


