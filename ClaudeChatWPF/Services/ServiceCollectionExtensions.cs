// Services/ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeChatWPF.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClaudeServices(this IServiceCollection services)
        {
            services.AddSingleton<AppConfig>();
            services.AddSingleton<IClaudeApiService, ClaudeApiService>();

            return services;
        }
    }
}