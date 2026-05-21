using FlowForge.Application.Dtos;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Queries.WebhookDeliveriesQuery
{
    //TODO: pagination eklenecek.
    public record WebhookDeliveryQuery(Guid? EndpointId) : IRequest<Result<List<WebhookDeliveryDto>>>;
}