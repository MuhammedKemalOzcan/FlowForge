using FlowForge.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace FlowForge.Infrastructure.Services
{
    public class CorrelationContext : ICorrelationContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid CorrelationId
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext is null) return Guid.NewGuid();

                if(httpContext.Items.TryGetValue("CorrelationId", out var value) && value is Guid correlationId)
                {
                    return correlationId;
                }

                return Guid.NewGuid();
            }
        }
    }
}
