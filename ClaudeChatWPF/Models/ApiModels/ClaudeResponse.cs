using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClaudeChatWPF.Models.ApiModels
{
    public class ClaudeResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public List<ClaudeContent> Content { get; set; } = new List<ClaudeContent>();

        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [JsonProperty("stop_reason")]
        public string StopReason { get; set; } = string.Empty;

        [JsonProperty("usage")]
        public ClaudeUsage Usage { get; set; } = new ClaudeUsage();
    }

    public class ClaudeContent
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class ClaudeUsage
    {
        [JsonProperty("input_tokens")]
        public int InputTokens { get; set; }

        [JsonProperty("output_tokens")]
        public int OutputTokens { get; set; }
    }
}