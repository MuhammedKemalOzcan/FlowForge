using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointRetryPolicy
{
    public record ChangeEndpointRetryPolicyCommand(Guid EndpointId, Guid TenantId, int MaxAttempts, BackoffStrategy Strategy, TimeSpan InitialDelay, TimeSpan MaxDelay, TimeSpan TimeOut) : IRequest<Result>;
}