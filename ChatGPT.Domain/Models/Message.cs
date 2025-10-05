using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatGPT.Domain.Models;

public class Message
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string ThreadId { get; set; } = default!;

    public ChatRole Role { get; set; }

    public string Content { get; set; } = string.Empty;

    public ChatProvider? Provider { get; set; }

    public string? Model { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }

    public Usage? Usage { get; set; }

    public string Status { get; set; } = "completed"; // completed | partial | failed

    public DateTime CreatedAt { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentMessageId { get; set; }
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}


