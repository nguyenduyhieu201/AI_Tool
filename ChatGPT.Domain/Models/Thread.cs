using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatGPT.Domain.Models;

public class Thread
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    public string Title { get; set; } = string.Empty;

    public ChatProvider Provider { get; set; }

    public string Model { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = default!;

    public string UserName { get; set; } = string.Empty;

    public int MessageCount { get; set; }

    public string? LastMessagePreview { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool Archived { get; set; }
}


