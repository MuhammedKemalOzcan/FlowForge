namespace FlowForge.Application.Abstractions
{
    public interface ICorrelationContext
    {
        Guid CorrelationId { get; }
    }
}