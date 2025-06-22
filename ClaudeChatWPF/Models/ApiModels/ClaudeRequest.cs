using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClaudeChatWPF.Models.ApiModels
{
    public class ClaudeRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = "claude-3-5-sonnet-20241022";

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = 4096;

        [JsonProperty("messages")]
        public List<ClaudeMessage> Messages { get; set; } = new List<ClaudeMessage>();
    }
}