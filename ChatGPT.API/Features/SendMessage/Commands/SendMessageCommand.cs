using MediatR;

namespace ChatGPT.API.Features.SendMessage.Commands;

public record SendMessageCommand(
    string UserId,
    string Message,
    string? ConversationId = null
) : IRequest<SendMessageResponse>;

public record SendMessageResponse(
    string Response,
    string ConversationId,
    DateTime Timestamp,
    int TokenCount
);


