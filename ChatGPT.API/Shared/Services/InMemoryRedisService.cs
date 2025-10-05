using ChatGPT.API.Shared.Models;
using ChatGPT.API.Shared.Services;
using System.Collections.Concurrent;

namespace ChatGPT.API.Shared.Services
{
    // Simple in-memory implementation to unblock DI and local testing
    public class InMemoryRedisService : IRedisService
    {
        private readonly ConcurrentDictionary<string, Conversation> _conversations = new();
        private readonly ConcurrentDictionary<string, List<string>> _userConversations = new();
        private readonly ConcurrentDictionary<string, ChatMessage> _messages = new();

        public Task<Conversation?> GetConversationAsync(string conversationId)
        {
            _conversations.TryGetValue(conversationId, out var conv);
            return Task.FromResult(conv);
        }

        public Task StoreConversationAsync(Conversation conversation)
        {
            _conversations[conversation.Id] = conversation;
            if (!string.IsNullOrEmpty(conversation.UserId))
            {
                _userConversations.AddOrUpdate(
                    conversation.UserId,
                    _ => new List<string> { conversation.Id },
                    (_, list) => {
                        if (!list.Contains(conversation.Id)) list.Add(conversation.Id);
                        return list;
                    }
                );
            }
            return Task.CompletedTask;
        }

        public Task UpdateConversationAsync(Conversation conversation)
        {
            _conversations[conversation.Id] = conversation;
            return Task.CompletedTask;
        }

        public Task StoreMessageAsync(ChatMessage message)
        {
            _messages[message.Id] = message;
            if (_conversations.TryGetValue(message.ConversationId, out var conv))
            {
                conv.Messages.Add(message);
                conv.UpdatedAt = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }

        public Task<List<ChatMessage>> GetUserMessagesAsync(string userId, int limit)
        {
            if (_userConversations.TryGetValue(userId, out var ids))
            {
                var msgs = ids
                    .SelectMany(id => _conversations.TryGetValue(id, out var c) ? c.Messages : new List<ChatMessage>())
                    .OrderBy(m => m.Timestamp)
                    .TakeLast(limit)
                    .ToList();
                return Task.FromResult(msgs);
            }
            return Task.FromResult(new List<ChatMessage>());
        }

        public Task AddConversationToUserAsync(string userId, string conversationId)
        {
            _userConversations.AddOrUpdate(
                userId,
                _ => new List<string> { conversationId },
                (_, list) => {
                    if (!list.Contains(conversationId)) list.Add(conversationId);
                    return list;
                }
            );
            return Task.CompletedTask;
        }

        public Task<List<string>> GetUserConversationsAsync(string userId)
        {
            if (_userConversations.TryGetValue(userId, out var ids))
            {
                return Task.FromResult(ids.ToList());
            }
            return Task.FromResult(new List<string>());
        }
    }
}
