using BuildingBlock.Domain;
using ChatGPT.Application.DTOs;
using ChatGPT.Application.Services;
using ChatGPT.Domain.Models;
using ChatGPT.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;
using Thread = ChatGPT.Domain.Models.Thread;
namespace ChatGPT.Infrastructure.Services;

public class ThreadService : IThreadService
{
    private readonly MongoContext _db;

    public ThreadService(MongoContext db)
    {
        _db = db;
    }

    public async Task<Result<CreateThreadResponse>> CreateThreadAsync(CreateThreadRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.UserName))
            return Result<CreateThreadResponse>.Fail("BAD_REQUEST: Missing userId or userName");

        if (!Enum.TryParse<ChatProvider>(request.Provider, true, out var provider))
            return Result<CreateThreadResponse>.Fail("BAD_REQUEST: Invalid provider");

        var now = DateTime.UtcNow;
        var thread = new Domain.Models.Thread
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Title = string.IsNullOrWhiteSpace(request.Title) ? "Cuộc trò chuyện mới" : request.Title!.Trim(),
            Provider = provider,
            Model = request.Model,
            UserId = request.UserId,
            UserName = request.UserName,
            MessageCount = 0,
            CreatedAt = now,
            UpdatedAt = now,
            Archived = false
        };

        await _db.Threads.InsertOneAsync(thread, cancellationToken: ct);

        var resp = new CreateThreadResponse(
            thread.Id,
            thread.Title,
            thread.Provider.ToString().ToLowerInvariant(),
            thread.Model,
            thread.UserId,
            thread.UserName,
            thread.CreatedAt,
            thread.UpdatedAt,
            thread.MessageCount
        );
        return Result<CreateThreadResponse>.Success(resp);
    }

    public async Task<Result<GetThreadResponse>> GetThreadAsync(string threadId, int page, int pageSize, string order, string userId, CancellationToken ct)
    {
        var thread = await _db.Threads.Find(x => x.Id == threadId).FirstOrDefaultAsync(ct);
        if (thread == null)
            return Result<GetThreadResponse>.Fail("NOT_FOUND: Thread not found");
        if (thread.UserId != userId)
            return Result<GetThreadResponse>.Fail("FORBIDDEN: Thread not owned by user");

        var sort = order.Equals("desc", StringComparison.OrdinalIgnoreCase)
            ? Builders<Message>.Sort.Descending(x => x.CreatedAt)
            : Builders<Message>.Sort.Ascending(x => x.CreatedAt);

        var skip = Math.Max(0, (page - 1) * pageSize);
        var messages = await _db.Messages
            .Find(x => x.ThreadId == threadId)
            .Sort(sort)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(ct);

        var hasNext = await _db.Messages.CountDocumentsAsync(x => x.ThreadId == threadId, cancellationToken: ct) > skip + messages.Count;

        var threadDto = new ThreadDto(thread.Id, thread.Title, thread.Provider.ToString().ToLowerInvariant(), thread.Model,
            thread.UserId, thread.UserName, thread.CreatedAt, thread.UpdatedAt, thread.MessageCount);

        var msgDtos = messages.Select(m => new ChatMessageDto(
            m.Id,
            m.Role.ToString().ToLowerInvariant(),
            m.Content,
            m.Provider?.ToString().ToLowerInvariant(),
            m.Model,
            m.CreatedAt,
            m.Usage != null ? new UsageDto(m.Usage.PromptTokens, m.Usage.CompletionTokens, m.Usage.TotalTokens) : null
        )).ToList();

        return Result<GetThreadResponse>.Success(new GetThreadResponse(
            threadDto,
            msgDtos,
            new PaginationDto(page, pageSize, hasNext)
        ));
    }

    public async Task<Result<ListThreadsResponse>> ListThreadsAsync(ListThreadsQuery query, CancellationToken ct)
    {
        var filter = Builders<Domain.Models.Thread>.Filter.Eq(x => x.UserId, query.UserId);
        if (!string.IsNullOrWhiteSpace(query.Provider) && Enum.TryParse<ChatProvider>(query.Provider, true, out var provider))
        {
            filter &= Builders<Thread>.Filter.Eq(x => x.Provider, provider);
        }
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            filter &= Builders<Thread>.Filter.Regex(x => x.Title, new MongoDB.Bson.BsonRegularExpression(query.Q, "i"));
        }

        var sort = query.SortBy.Equals("createdAt", StringComparison.OrdinalIgnoreCase)
            ? (query.Order.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? Builders<Thread>.Sort.Ascending(x => x.CreatedAt)
                : Builders<Thread>.Sort.Descending(x => x.CreatedAt))
            : (query.Order.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? Builders<Thread>.Sort.Ascending(x => x.UpdatedAt)
                : Builders<Thread>.Sort.Descending(x => x.UpdatedAt));

        var skip = Math.Max(0, (query.Page - 1) * query.PageSize);

        var total = await _db.Threads.CountDocumentsAsync(filter, cancellationToken: ct);
        var items = await _db.Threads.Find(filter)
            .Sort(sort)
            .Skip(skip)
            .Limit(query.PageSize)
            .ToListAsync(ct);

        var dtoItems = items.Select(t => new ThreadListItemDto(
            t.Id,
            t.Title,
            t.Provider.ToString().ToLowerInvariant(),
            t.Model,
            t.UserId,
            t.CreatedAt,
            t.UpdatedAt,
            t.MessageCount,
            t.LastMessagePreview
        )).ToList();

        return Result<ListThreadsResponse>.Success(new ListThreadsResponse(
            dtoItems,
            new PaginationTotalDto(query.Page, query.PageSize, total)
        ));
    }
}




