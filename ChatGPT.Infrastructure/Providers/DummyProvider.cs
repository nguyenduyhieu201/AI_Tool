using BuildingBlock.Domain;
using ChatGPT.Application.DTOs;
using ChatGPT.Application.Services;

namespace ChatGPT.Infrastructure.Providers;

// Simple placeholder provider that echoes back; replace with real OpenAI/DeepSeek integrations
public class DummyProvider : IAIProvider
{
    public Task<Result<ProviderReply>> ChatAsync(ProviderRequest request, CancellationToken ct)
    {
        var userLast = request.Messages.LastOrDefault(m => m.role == "user").content;
        var content = $"[dummy-{request.Provider}:{request.Model}] {userLast}";
        var usage = new UsageDto(20, 40, 60);
        return Task.FromResult(Result<ProviderReply>.Success(new ProviderReply(content, usage)));
    }
}




