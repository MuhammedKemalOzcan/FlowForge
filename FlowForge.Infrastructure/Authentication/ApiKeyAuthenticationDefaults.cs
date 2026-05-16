using MassTransit.Internals.GraphValidation;
using Microsoft.AspNetCore.Authorization;

namespace FlowForge.Infrastructure.Authentication
{
    public class ApiKeyAuthenticationDefaults
    {
        //Şunu istemiyoruz:[Authorize(AuthenticationSchemes = "ApiKey")]
        //bir yerde "ApiKey", başka yerde "apikey", başka yerde "Api-Key" yazılsın.
        public const string AuthenticationScheme = "ApiKey";

        public const string HeaderName = "X-Api-Key";
    }
}