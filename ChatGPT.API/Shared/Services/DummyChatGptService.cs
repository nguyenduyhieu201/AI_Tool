using ChatGPT.API.Shared.Models;

namespace ChatGPT.API.Shared.Services
{
    // Dummy implementation to unblock DI and local testing
    public class DummyChatGptService : IChatGptService
    {
        public Task<string> GenerateResponseAsync(string userMessage, List<ChatMessage> conversationHistory, UserInfoDto user)
        {
            var reply = $"[dummy] Hello {user.FirstName}, you said: '{userMessage}'. Total history messages: {conversationHistory?.Count ?? 0}.";
            return Task.FromResult(reply);
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(string userMessage, List<ChatMessage> conversationHistory, UserInfoDto user, CancellationToken cancellationToken)
        {
            var parts = new []
            {
                "[dummy-stream] ",
                $"Hello {user.FirstName}. ",
                "You said: ",
                $"'{userMessage}'. ",
                $"History count: {conversationHistory?.Count ?? 0}."
            };
            foreach (var p in parts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100, cancellationToken);
                yield return p;
            }
        }
    }
}
