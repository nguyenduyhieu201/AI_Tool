using BuildingBlock.CQRS;

namespace ChatGPT.API.Features.SendMessage.Queries;

public record GetMessageHistoryQuery(
    string UserId,
    string ConversationId,
    int Limit = 50
) : IQuery<GetMessageHistoryResponse>;

public record GetMessageHistoryResponse(
    List<MessageDto> Messages,
    int TotalCount
);

public record MessageDto(
    string Id,
    string Content,
    string Role,
    DateTime Timestamp,
    int TokenCount
);


