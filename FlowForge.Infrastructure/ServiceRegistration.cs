using FlowForge.Domain.Services;
using FlowForge.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FlowForge.Infrastructure
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddHttpClient<IWebhookSender, WebhookSender>();
        }
    }
}