// Services/IClaudeApiService.cs
using ClaudeChatWPF.Models.ApiModels;

namespace ClaudeChatWPF.Services
{
    public interface IClaudeApiService
    {
        Task<string> SendMessageAsync(List<ClaudeMessage> messages);
        Task<ClaudeResponse> SendMessageDetailedAsync(List<ClaudeMessage> messages);
        bool IsConfigured { get; }
    }
}