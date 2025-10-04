using System.Net.Http.Json;

namespace ChatGPTTool.Services;

public interface IChatApiClient
{
	Task<ListThreadsResponse?> ListThreadsAsync(int page = 1, int pageSize = 50, string order = "desc", CancellationToken ct = default);
	Task<GetThreadResponse?> GetThreadAsync(string threadId, int page = 1, int pageSize = 100, string order = "asc", CancellationToken ct = default);
	Task<CreateThreadResponse?> CreateThreadAsync(CreateThreadRequest request, CancellationToken ct = default);
	Task<SendMessageResponse?> SendMessageAsync(string threadId, SendMessageRequest request, CancellationToken ct = default);
}

public class ChatApiClient : IChatApiClient
{
	private readonly IHttpClientFactory _httpClientFactory;

	public ChatApiClient(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	private HttpClient CreateClient()
	{
		return _httpClientFactory.CreateClient("Gateway");
	}

	public async Task<ListThreadsResponse?> ListThreadsAsync(int page = 1, int pageSize = 50, string order = "desc", CancellationToken ct = default)
	{
		var client = CreateClient();
		var url = $"/api/gpt/threads?page={page}&pageSize={pageSize}&order={order}";
		return await client.GetFromJsonAsync<ListThreadsResponse>(url, ct);
	}

	public async Task<GetThreadResponse?> GetThreadAsync(string threadId, int page = 1, int pageSize = 100, string order = "asc", CancellationToken ct = default)
	{
		var client = CreateClient();
		var url = $"/api/gpt/threads/{threadId}?page={page}&pageSize={pageSize}&order={order}";
		return await client.GetFromJsonAsync<GetThreadResponse>(url, ct);
	}

	public async Task<CreateThreadResponse?> CreateThreadAsync(CreateThreadRequest request, CancellationToken ct = default)
	{
		var client = CreateClient();
		var resp = await client.PostAsJsonAsync("/api/gpt/threads", request, ct);
		resp.EnsureSuccessStatusCode();
		return await resp.Content.ReadFromJsonAsync<CreateThreadResponse>(cancellationToken: ct);
	}

	public async Task<SendMessageResponse?> SendMessageAsync(string threadId, SendMessageRequest request, CancellationToken ct = default)
	{
		var client = CreateClient();
		var resp = await client.PostAsJsonAsync($"/api/gpt/threads/{threadId}/messages", request, ct);
		resp.EnsureSuccessStatusCode();
		return await resp.Content.ReadFromJsonAsync<SendMessageResponse>(cancellationToken: ct);
	}
}

// Minimal DTOs matching ChatGPT.API
public record CreateThreadRequest(string UserId, string UserName, string Provider, string Model, string? Title);
public record CreateThreadResponse(string ThreadId, string Title, string Provider, string Model, string UserId, string UserName, DateTime CreatedAt, DateTime UpdatedAt, int MessageCount);

public record SendMessageRequest(string UserId, string Content, Dictionary<string, object>? Metadata, bool Stream = false);

public record ChatMessageDto(string MessageId, string Role, string Content, string? Provider, string? Model, DateTime CreatedAt, UsageDto? Usage);
public record UsageDto(int PromptTokens, int CompletionTokens, int TotalTokens);

public record SendMessageResponse(string ThreadId, ChatMessageDto UserMessage, ChatMessageDto AssistantMessage, DateTime UpdatedAt, bool AutoRenamed);

public record ThreadDto(string ThreadId, string Title, string Provider, string Model, string UserId, string UserName, DateTime CreatedAt, DateTime UpdatedAt, int MessageCount);
public record PaginationDto(int Page, int PageSize, bool HasNext);
public record GetThreadResponse(ThreadDto Thread, IReadOnlyList<ChatMessageDto> Messages, PaginationDto Pagination);

public record ThreadListItemDto(string ThreadId, string Title, string Provider, string Model, string UserId, DateTime CreatedAt, DateTime UpdatedAt, int MessageCount, string? LastMessagePreview);
public record PaginationTotalDto(int Page, int PageSize, long Total);
public record ListThreadsResponse(IReadOnlyList<ThreadListItemDto> Items, PaginationTotalDto Pagination);



