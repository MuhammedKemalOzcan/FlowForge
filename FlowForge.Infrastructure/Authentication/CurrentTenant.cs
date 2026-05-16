using FlowForge.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace FlowForge.Infrastructure.Authentication
{
    internal class CurrentTenant : ICurrentTenant
    {
        private readonly IHttpContextAccessor _httpContext;

        public CurrentTenant(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        public Guid? TenantId => ReadGuidClaim(ClaimNames.TenantId);

        public Guid? ApiKeyId => ReadGuidClaim(ClaimNames.ApiKeyId);

        public Guid GetRequiredTenantId()
        {
            return TenantId ?? throw new InvalidOperationException("TenantId is required but not present in the current context. " +
                "Ensure the endpoint is protected with [Authorize(AuthenticationSchemes = \"ApiKey\")] " +
                "or that the request was made within an authenticated HTTP context.");
        }

        private Guid? ReadGuidClaim(string claimType)
        {
            var claimValue = _httpContext.HttpContext?
                .User?
                .FindFirst(claimType)?
                .Value;

            if (string.IsNullOrEmpty(claimValue)) return null;

            return Guid.TryParse(claimValue, out Guid parsed) ? parsed : null;
        }
    }
}