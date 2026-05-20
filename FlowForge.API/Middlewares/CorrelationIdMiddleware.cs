using Serilog.Context;

namespace FlowForge.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeaderName = "X-Correlation-Id";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrCreateCorrelationId(context);

            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers[CorrelationIdHeaderName] = correlationId.ToString();

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private static Guid GetOrCreateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) && Guid.TryParse(correlationId, out var parsed))
            {
                return parsed;
            }

            return Guid.NewGuid();
        }
    }
}