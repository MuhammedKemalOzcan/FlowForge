using FlowForge.Application.Dtos;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Queries.WebhookDeliveriesQuery.GetDeliveryById;

public record GetDeliveryByIdQuery(Guid DeliveryId) : IRequest<Result<WebhookDeliveryDto>>;
