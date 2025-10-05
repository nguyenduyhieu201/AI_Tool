using BuildingBlock.Domain;
using ChatGPT.Application.DTOs;
using ChatGPT.Application.Services;
using ChatGPT.Domain.Models;
using ChatGPT.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;
using Thread = ChatGPT.Domain.Models.Thread;

namespace ChatGPT.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly MongoContext _db;
    private readonly IAIProvider _provider;

    public MessageService(MongoContext db, IAIProvider provider)
    {
        _db = db;
        _provider = provider;
    }

    public async Task<Result<SendMessageResponse>> SendMessageAsync(string threadId, SendMessageRequest request, CancellationToken ct)
    {
        var thread = await _db.Threads.Find(x => x.Id == threadId).FirstOrDefaultAsync(ct);
        if (thread == null)
            return Result<SendMessageResponse>.Fail("NOT_FOUND: Thread not found");
        if (thread.UserId != request.UserId)
            return Result<SendMessageResponse>.Fail("FORBIDDEN: Thread not owned by user");

        var now = DateTime.UtcNow;
        var userMsg = new Message
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ThreadId = threadId,
            Role = ChatRole.User,
            Content = request.Content,
            UserId = request.UserId,
            Metadata = request.Metadata,
            CreatedAt = now
        };
        await _db.Messages.InsertOneAsync(userMsg, cancellationToken: ct);

        // Build context messages (last N)
        var history = await _db.Messages.Find(m => m.ThreadId == threadId)
            .SortBy(m => m.CreatedAt)
            .Limit(20)
            .ToListAsync(ct);
        var providerReq = new ProviderRequest(
            thread.Provider.ToString().ToLowerInvariant(),
            thread.Model,
            history.Select(h => (h.Role.ToString().ToLowerInvariant(), h.Content)).Append(("user", request.Content)).ToList()
        );

        var aiResult = await _provider.ChatAsync(providerReq, ct);
        if (aiResult.IsFail)
            return Result<SendMessageResponse>.Fail(aiResult.Error!);

        var reply = aiResult.Value!;
        var assistantMsg = new Message
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ThreadId = threadId,
            Role = ChatRole.Assistant,
            Content = reply.Content,
            Provider = thread.Provider,
            Model = thread.Model,
            Usage = reply.Usage != null ? new Usage
            {
                PromptTokens = reply.Usage.PromptTokens,
                CompletionTokens = reply.Usage.CompletionTokens,
                TotalTokens = reply.Usage.TotalTokens
            } : null,
            CreatedAt = DateTime.UtcNow
        };
        await _db.Messages.InsertOneAsync(assistantMsg, cancellationToken: ct);

        // Update thread
        var update = Builders<Thread>.Update
            .Inc(t => t.MessageCount, 2)
            .Set(t => t.LastMessagePreview, assistantMsg.Content.Length > 100 ? assistantMsg.Content[..100] : assistantMsg.Content)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);
        var filterById = Builders<Thread>.Filter.Eq(t => t.Id, threadId);
        await _db.Threads.UpdateOneAsync(filterById, update, new UpdateOptions(), ct);

        bool autoRenamed = false;
        if (thread.Title == "Cuộc trò chuyện mới")
        {
            var newTitle = request.Content.Length > 60 ? request.Content[..60] : request.Content;
            var titleUpdate = Builders<Domain.Models.Thread>.Update.Set(t => t.Title, newTitle);
            await _db.Threads.UpdateOneAsync(filterById, titleUpdate, new UpdateOptions(), ct);
            autoRenamed = true;
        }

        var userDto = new ChatMessageDto(userMsg.Id, "user", userMsg.Content, null, null, userMsg.CreatedAt, null);
        var assistantDto = new ChatMessageDto(assistantMsg.Id, "assistant", assistantMsg.Content,
            assistantMsg.Provider?.ToString().ToLowerInvariant(), assistantMsg.Model!, assistantMsg.CreatedAt,
            assistantMsg.Usage != null ? new UsageDto(assistantMsg.Usage.PromptTokens, assistantMsg.Usage.CompletionTokens, assistantMsg.Usage.TotalTokens) : null);

        return Result<SendMessageResponse>.Success(new SendMessageResponse(
            threadId,
            userDto,
            assistantDto,
            DateTime.UtcNow,
            autoRenamed
        ));
    }
}




