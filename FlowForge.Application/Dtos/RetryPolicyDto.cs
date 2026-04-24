using FlowForge.Domain.Enums;

namespace FlowForge.Application.Dtos
{
    public class RetryPolicyDto
    {
        public int MaxAttempts { get; set; }
        public BackoffStrategy Strategy { get; set; }
        public TimeSpan InitialDelay { get; set; }
        public TimeSpan MaxDelay { get; set; }
        public TimeSpan Timeout { get; set; }
    }
}