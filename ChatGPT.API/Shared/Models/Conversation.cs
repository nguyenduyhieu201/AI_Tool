namespace ChatGPT.API.Shared.Models;

public class Conversation
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class ChatMessage
{
    public string Id { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "user", "assistant", "system"
    public DateTime Timestamp { get; set; }
    public int TokenCount { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class UserContext
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; }
    public string CurrentConversationId { get; set; } = string.Empty;
    public List<string> RecentConversations { get; set; } = new();
    public Dictionary<string, object> Preferences { get; set; } = new();
}


