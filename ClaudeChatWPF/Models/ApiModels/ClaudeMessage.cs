using Newtonsoft.Json;

namespace ClaudeChatWPF.Models.ApiModels
{
    public class ClaudeMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
    }
}