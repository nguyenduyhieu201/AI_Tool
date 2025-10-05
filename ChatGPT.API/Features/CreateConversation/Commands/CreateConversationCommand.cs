using BuildingBlock.CQRS;

namespace ChatGPT.API.Features.CreateConversation.Commands;

public record CreateConversationCommand(
    string UserId,
    string Title = "New Conversation"
) : ICommand<CreateConversationResponse>;

public record CreateConversationResponse(
    string ConversationId,
    string Title,
    DateTime CreatedAt
);


