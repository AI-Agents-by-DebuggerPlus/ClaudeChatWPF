// Services/ClaudeApiService.cs
using ClaudeChatWPF.Models.ApiModels;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace ClaudeChatWPF.Services
{
    public class ClaudeApiService : IClaudeApiService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly AppConfig _config;
        private bool _disposed = false;

        public bool IsConfigured => !string.IsNullOrEmpty(_config.ClaudeApiKey);

        public ClaudeApiService(AppConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_config.ClaudeBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _config.ClaudeApiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ClaudeChatWPF/1.0");
        }

        public async Task<string> SendMessageAsync(List<ClaudeMessage> messages)
        {
            try
            {
                var response = await SendMessageDetailedAsync(messages);

                if (response?.Content?.Count > 0)
                {
                    return response.Content.First().Text;
                }

                return "Извините, получен пустой ответ от Claude API.";
            }
            catch (Exception ex)
            {
                return $"Ошибка при обращении к Claude API: {ex.Message}";
            }
        }

        public async Task<ClaudeResponse> SendMessageDetailedAsync(List<ClaudeMessage> messages)
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException("Claude API не настроен. Проверьте API ключ в конфигурации.");
            }

            if (messages == null || messages.Count == 0)
            {
                throw new ArgumentException("Список сообщений не может быть пустым.", nameof(messages));
            }

            var request = new ClaudeRequest
            {
                Model = _config.ClaudeModelName,
                MaxTokens = _config.ClaudeMaxTokens,
                Messages = messages
            };

            var json = JsonConvert.SerializeObject(request, Formatting.None);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/v1/messages", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Claude API вернул ошибку {response.StatusCode}: {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var claudeResponse = JsonConvert.DeserializeObject<ClaudeResponse>(responseJson);

                if (claudeResponse == null)
                {
                    throw new InvalidOperationException("Не удалось десериализовать ответ от Claude API.");
                }

                return claudeResponse;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new TimeoutException("Время ожидания ответа от Claude API истекло.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Ошибка при обработке JSON ответа от Claude API.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Неожиданная ошибка при обращении к Claude API.", ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}