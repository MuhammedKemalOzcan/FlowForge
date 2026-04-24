using FlowForge.Application.Dtos;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Queries.WebhookDeliveriesQuery
{
    public record WebhookDeliveryQuery(Guid TenantId) : IRequest<Result<List<WebhookDeliveryDto>>>;
}