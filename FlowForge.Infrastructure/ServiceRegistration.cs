using FlowForge.Application.Abstractions;
using FlowForge.Application.Consumers;
using FlowForge.Domain.Services;
using FlowForge.Infrastructure.Authentication;
using FlowForge.Infrastructure.Services;
using FlowForge.Infrastructure.Streaming;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;

namespace FlowForge.Infrastructure
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IWebhookSender, WebhookSender>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy())
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30)));

            //IConnectionMultiplexer Redis'e bağlantıyı yönetiyor — singleton olarak kayıt doğru çünkü tek bir bağlantı havuzu yönetir, her request'te yeni bağlantı açmaz.
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]));

            //SSE canlı akış hub'ı — tüm delivery olayları tek process içinde fan-out edildiği için singleton in-memory.
            services.AddSingleton<IWebhookDeliveryStreamBroadcaster, InMemoryWebhookDeliveryStreamBroadcaster>();

            services.AddScoped<IRateLimiter, RedisRateLimiter>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentTenant, CurrentTenant>();
            services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
            services.AddScoped<IApiKeyValidationCache, ApiKeyValidationCache>();
            services.AddScoped<ICorrelationContext, CorrelationContext>();

            services.AddMassTransit(bus =>
            {
                bus.AddConsumer<ProcessWebhookDeliveryConsumer>();

                bus.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMQ:HostName"], "/", h =>
                    {
                        h.Username(configuration["RabbitMQ:UserName"]);
                        h.Password(configuration["RabbitMQ:Password"]);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"Polly Retry {retryAttempt} after {timespan.TotalSeconds:F1}s - Status: {outcome.Result?.StatusCode}");
                });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,    // 5 başarısız
                    durationOfBreak: TimeSpan.FromSeconds(30)); // 30sn bekle
        }
    }
}