using FlowForge.Domain.Entities;

namespace FlowForge.Domain.ValueObjects
{
    public record ApiKeyCreationResult(string RawKey, ApiKey ApiKey);
}