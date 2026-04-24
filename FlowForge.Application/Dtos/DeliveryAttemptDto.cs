using FlowForge.Domain.Enums;
using System.Net;

namespace FlowForge.Application.Dtos
{
    public class DeliveryAttemptDto
    {
        public Guid Id { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public long DurationMs { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public string? ErrorMessage { get; set; }
        public OutcomeStatus OutcomeStatus { get; set; }
    }
}