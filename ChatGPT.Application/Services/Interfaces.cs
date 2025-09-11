using BuildingBlock.Domain;
using ChatGPT.Application.DTOs;

namespace ChatGPT.Application.Services;

public interface IThreadService
{
    Task<Result<CreateThreadResponse>> CreateThreadAsync(CreateThreadRequest request, CancellationToken ct);
    Task<Result<GetThreadResponse>> GetThreadAsync(string threadId, int page, int pageSize, string order, string userId, CancellationToken ct);
    Task<Result<ListThreadsResponse>> ListThreadsAsync(ListThreadsQuery query, CancellationToken ct);
}

public interface IMessageService
{
    Task<Result<SendMessageResponse>> SendMessageAsync(string threadId, SendMessageRequest request, CancellationToken ct);
}

public interface IAIProvider
{
    Task<Result<ProviderReply>> ChatAsync(ProviderRequest request, CancellationToken ct);
}

public record ProviderRequest(string Provider, string Model, IReadOnlyList<(string role, string content)> Messages);
public record ProviderReply(string Content, UsageDto? Usage);



