using MediatR;

namespace ChatGPT.API.Features.StreamChat.Commands;

public record StreamChatCommand(
    string UserId,
    string Message,
    string? ConversationId = null
) : IRequest<IAsyncEnumerable<string>>;

public record StreamChatChunk(
    string Content,
    bool IsComplete,
    DateTime Timestamp
);


