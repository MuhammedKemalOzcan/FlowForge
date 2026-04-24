using FlowForge.Application.Data;
using FlowForge.Domain.Repositories;
using FlowForge.Persistence.Contexts;
using FlowForge.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowForge.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FlowForgeAPIDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FlowForgeAPIDbContext>());
            services.AddScoped<IFlowForgeApiDbContext>(provider => provider.GetRequiredService<FlowForgeAPIDbContext>());
            services.AddScoped<IWebhookEndpointRepository, WebhookEndpointRepository>();
            services.AddScoped<IWebhookDeliveryRepository, WebhookDeliveryRepository>();
            services.AddScoped<IWebhookDeliveryRepository, WebhookDeliveryRepository>();
        }
    }
}