using MediatR;
using ChatGPT.API.Features.CreateConversation.Commands;
using ChatGPT.API.Shared.Services;
using ChatGPT.API.Shared.Models;

namespace ChatGPT.API.Features.CreateConversation.Handlers;

public class CreateConversationHandler : IRequestHandler<CreateConversationCommand, CreateConversationResponse>
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly IRedisService _redisService;
    private readonly ILogger<CreateConversationHandler> _logger;

    public CreateConversationHandler(
        IUserServiceClient userServiceClient,
        IRedisService redisService,
        ILogger<CreateConversationHandler> logger)
    {
        _userServiceClient = userServiceClient;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<CreateConversationResponse> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate user
            var user = await _userServiceClient.GetUserByIdAsync(request.UserId, "token");
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // 2. Create new conversation
            var conversation = new Conversation
            {
                Id = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                Title = request.Title,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Messages = new List<ChatMessage>(),
                IsActive = true
            };

            // 3. Store conversation
            await _redisService.StoreConversationAsync(conversation);

            // 4. Add to user's conversation list
            await _redisService.AddConversationToUserAsync(request.UserId, conversation.Id);

            _logger.LogInformation("Created conversation {ConversationId} for user {UserId}", 
                conversation.Id, request.UserId);

            return new CreateConversationResponse(
                conversation.Id,
                conversation.Title,
                conversation.CreatedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation for user {UserId}", request.UserId);
            throw;
        }
    }
}


