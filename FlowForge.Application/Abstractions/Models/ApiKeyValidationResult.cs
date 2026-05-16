namespace FlowForge.Application.Abstractions.Models
{
    public record ApiKeyValidationResult(Guid TenantId, Guid ApiKeyId);
}