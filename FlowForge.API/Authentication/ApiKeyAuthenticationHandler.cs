using FlowForge.Application.Abstractions;
using FlowForge.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FlowForge.API.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IApiKeyValidator _apiKeyValidator;

        public ApiKeyAuthenticationHandler(
           IOptionsMonitor<ApiKeyAuthenticationOptions> options,
           ILoggerFactory logger,
           UrlEncoder encoder,
           IApiKeyValidator apiKeyValidator)
           : base(options, logger, encoder)
        {
            _apiKeyValidator = apiKeyValidator;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.HeaderName, out var apiKeyHeaderValues))
                return AuthenticateResult.Fail("API key header is missing");

            if (apiKeyHeaderValues.Count != 1)
                return AuthenticateResult.Fail("Invalid API key header.");

            var plainApiKey = apiKeyHeaderValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(plainApiKey))
                return AuthenticateResult.Fail("API key is missing");

            var validationResult = await _apiKeyValidator.ValidateAsync(plainApiKey, Context.RequestAborted);

            if (!validationResult.IsSuccess)
                return AuthenticateResult.Fail("Invalid API key.");

            var claims = new List<Claim>
            {
                new Claim(ClaimNames.TenantId, validationResult.Data.TenantId.ToString()),
                new Claim(ClaimNames.ApiKeyId, validationResult.Data.ApiKeyId.ToString())
            };

            var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            //Ticket bilgisi Authentication başarılı, user bilgisi bu ve schme bu diye döner.
            var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationDefaults.AuthenticationScheme);

            return AuthenticateResult.Success(ticket);
        }
    }
}