using MediatR;

namespace ChatGPT.API.Features.CreateConversation.Commands;

public record CreateConversationCommand(
    string UserId,
    string Title = "New Conversation"
) : IRequest<CreateConversationResponse>;

public record CreateConversationResponse(
    string ConversationId,
    string Title,
    DateTime CreatedAt
);


