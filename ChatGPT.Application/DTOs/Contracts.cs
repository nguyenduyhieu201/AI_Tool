using BuildingBlock.Domain;
using ChatGPT.Domain.Models;

namespace ChatGPT.Application.DTOs;

public record CreateThreadRequest(
    string UserId,
    string UserName,
    string Provider,
    string Model,
    string? Title
);

public record CreateThreadResponse(
    string ThreadId,
    string Title,
    string Provider,
    string Model,
    string UserId,
    string UserName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int MessageCount
);

public record SendMessageRequest(
    string UserId,
    string Content,
    Dictionary<string, object>? Metadata,
    bool Stream = false
);

public record ChatMessageDto(
    string MessageId,
    string Role,
    string Content,
    string? Provider,
    string? Model,
    DateTime CreatedAt,
    UsageDto? Usage
);

public record UsageDto(int PromptTokens, int CompletionTokens, int TotalTokens);

public record SendMessageResponse(
    string ThreadId,
    ChatMessageDto UserMessage,
    ChatMessageDto AssistantMessage,
    DateTime UpdatedAt,
    bool AutoRenamed
);

public record ThreadDto(
    string ThreadId,
    string Title,
    string Provider,
    string Model,
    string UserId,
    string UserName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int MessageCount
);

public record GetThreadResponse(
    ThreadDto Thread,
    IReadOnlyList<ChatMessageDto> Messages,
    PaginationDto Pagination
);

public record PaginationDto(int Page, int PageSize, bool HasNext);

public record ListThreadsQuery(
    string UserId,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "updatedAt",
    string Order = "desc",
    string? Provider = null,
    string? Q = null
);

public record ThreadListItemDto(
    string ThreadId,
    string Title,
    string Provider,
    string Model,
    string UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int MessageCount,
    string? LastMessagePreview
);

public record ListThreadsResponse(
    IReadOnlyList<ThreadListItemDto> Items,
    PaginationTotalDto Pagination
);

public record PaginationTotalDto(int Page, int PageSize, long Total);



