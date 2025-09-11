using BuildingBlock.Domain;
using ChatGPT.Application.DTOs;
using ChatGPT.Application.Services;
using ChatGPT.Infrastructure.Persistence;
using ChatGPT.Infrastructure.Providers;
using ChatGPT.Infrastructure.Services;
using ChatGPT.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Options
var mongoOptions = new MongoOptions
{
    ConnectionString = builder.Configuration.GetConnectionString("Mongo") ?? "mongodb://localhost:27017",
    Database = builder.Configuration.GetValue<string>("MongoDatabase") ?? "chatgpt_service"
};

// DI
builder.Services.AddSingleton(mongoOptions);
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<IAIProvider, DummyProvider>();
builder.Services.AddScoped<IThreadService, ThreadService>();
builder.Services.AddScoped<IMessageService, MessageService>();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Extract userId from JWT into HttpContext.Items["UserId"]
app.UseUserContext();


app.MapPost("/api/threads", async (HttpContext ctx, CreateThreadRequest req, IThreadService svc, CancellationToken ct) =>
{
    var userIdFromToken = ctx.GetUserId();
    if (!string.IsNullOrEmpty(userIdFromToken))
        req = req with { UserId = userIdFromToken };
    var result = await svc.CreateThreadAsync(req, ct);
    return ToHttp(result, created: true);
});

app.MapPost("/api/threads/{threadId}/messages", async (string threadId, HttpContext ctx, SendMessageRequest req, IMessageService svc, CancellationToken ct) =>
{
    var userIdFromToken = ctx.GetUserId();
    if (!string.IsNullOrEmpty(userIdFromToken))
        req = req with { UserId = userIdFromToken };
    var result = await svc.SendMessageAsync(threadId, req, ct);
    return ToHttp(result);
});

app.MapGet("/api/threads/{threadId}", async (string threadId, int page, int pageSize, string order, HttpContext ctx, IThreadService svc, CancellationToken ct) =>
{
    var userId = ctx.GetUserId();
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
    page = page <= 0 ? 1 : page; pageSize = pageSize <= 0 ? 50 : pageSize; order = string.IsNullOrEmpty(order) ? "asc" : order;
    var result = await svc.GetThreadAsync(threadId, page, pageSize, order, userId!, ct);
    return ToHttp(result);
});

app.MapGet("/api/threads", async (int page, int pageSize, string? sortBy, string? order, string? provider, string? q, HttpContext ctx, IThreadService svc, CancellationToken ct) =>
{
    var userId = ctx.GetUserId();
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
    var query = new ListThreadsQuery(userId!, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, sortBy ?? "updatedAt", order ?? "desc", provider, q);
    var result = await svc.ListThreadsAsync(query, ct);
    return ToHttp(result);
});

app.Run();

static IResult ToHttp<T>(Result<T> result, bool created = false)
{
    if (result.IsSuccess)
        return created ? Results.Created(string.Empty, result.Value) : Results.Ok(result.Value);

    var (code, message) = ParseError(result.Error);
    return code switch
    {
        "BAD_REQUEST" => Results.BadRequest(message),
        "UNAUTHORIZED" => Results.Unauthorized(),
        "FORBIDDEN" => Results.StatusCode(403),
        "NOT_FOUND" => Results.NotFound(message),
        "CONFLICT" => Results.Conflict(message),
        "RATE_LIMITED" => Results.StatusCode(429),
        "UPSTREAM_ERROR" => Results.StatusCode(502),
        "TIMEOUT" => Results.StatusCode(504),
        _ => Results.StatusCode(500)
    };
}

static (string code, string message) ParseError(string? error)
{
    if (string.IsNullOrEmpty(error)) return ("INTERNAL", "Internal error");
    var idx = error.IndexOf(':');
    if (idx <= 0) return (error.Trim().ToUpperInvariant(), error);
    var code = error[..idx].Trim().ToUpperInvariant();
    var msg = error[(idx + 1)..].Trim();
    return (code, msg);
}


