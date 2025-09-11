using ChatGPT.API.Shared.Models;

namespace ChatGPT.API.Shared.Services;

public interface IChatGptService
{
    Task<string> GenerateResponseAsync(string userMessage, List<ChatMessage> conversationHistory, UserInfoDto user);
    IAsyncEnumerable<string> StreamResponseAsync(string userMessage, List<ChatMessage> conversationHistory, UserInfoDto user, CancellationToken cancellationToken);
}

public interface IRedisService
{
    Task<Conversation?> GetConversationAsync(string conversationId);
    Task StoreConversationAsync(Conversation conversation);
    Task UpdateConversationAsync(Conversation conversation);
    Task StoreMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetUserMessagesAsync(string userId, int limit);
    Task AddConversationToUserAsync(string userId, string conversationId);
    Task<List<string>> GetUserConversationsAsync(string userId);
}


