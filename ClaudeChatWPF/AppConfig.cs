using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ClaudeChatWPF
{
    public class AppConfig
    {
        private readonly IConfiguration _configuration;

        public AppConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public string ClaudeApiKey => _configuration["Claude:ApiKey"] ?? string.Empty;
        public string ClaudeBaseUrl => _configuration["Claude:BaseUrl"] ?? "https://api.anthropic.com";
        public string ClaudeModelName => _configuration["Claude:ModelName"] ?? "claude-3-5-sonnet-20241022";
        public int ClaudeMaxTokens => int.Parse(_configuration["Claude:MaxTokens"] ?? "4096");
        public string DatabaseConnectionString => _configuration["Database:ConnectionString"] ?? "Data Source=Data/ChatDatabase.db";
    }
}