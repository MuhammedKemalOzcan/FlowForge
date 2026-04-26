using System.Net;

namespace FlowForge.Domain.Models
{
    public class WebhookSendResult
    {
        public bool IsSuccess { get; private set; }
        public HttpStatusCode? StatusCode { get; private set; }
        public long DurationMs { get; private set; }
        public string? ResponseBodySnippet { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime CompletedAt { get; private set; }

        public static WebhookSendResult Success(
            HttpStatusCode statusCode, long durationMs,
            string? responseBody, DateTime startedAt, DateTime completedAt)
        {
            return new WebhookSendResult
            {
                IsSuccess = true,
                StatusCode = statusCode,
                DurationMs = durationMs,
                ResponseBodySnippet = responseBody,
                StartedAt = startedAt,
                CompletedAt = completedAt
            };
        }

        public static WebhookSendResult Failure(
            HttpStatusCode? statusCode, long durationMs,
            string? errorMessage, DateTime startedAt, DateTime completedAt)
        {
            return new WebhookSendResult
            {
                IsSuccess = false,
                StatusCode = statusCode,
                DurationMs = durationMs,
                ErrorMessage = errorMessage,
                StartedAt = startedAt,
                CompletedAt = completedAt
            };
        }
    }
}