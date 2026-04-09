using FlowForge.Domain.Enums;
using System.Net;

namespace FlowForge.Domain.Entities
{
    public class DeliveryAttempt
    {
        //Webhook içerisinde yaşayan entity.
        public int AttemptNumber { get; private set; }

        public DateTime StartedAt { get; private set; }
        public DateTime CompletedAt { get; private set; }
        public long DurationMs { get; private set; }
        public HttpStatusCode? StatusCode { get; private set; }
        public string? ResponseBodySnippet { get; private set; }
        public string? ErrorMessage { get; private set; }
        public OutcomeStatus Outcome { get; private set; }

        private DeliveryAttempt()
        { }

        internal DeliveryAttempt(int attemptNumber, long durationMs, HttpStatusCode? statusCode, string? responseBodySnippet, string? errorMessage, DateTime startedAt, DateTime completedAt, OutcomeStatus outcomeStatus)
        {
            AttemptNumber = attemptNumber;
            StartedAt = startedAt;
            CompletedAt = completedAt;
            DurationMs = durationMs;
            StatusCode = statusCode;
            ResponseBodySnippet = responseBodySnippet?.Length > 500 ? responseBodySnippet[..500] : responseBodySnippet;
            ErrorMessage = errorMessage;
            Outcome = outcomeStatus;
        }
    }
}