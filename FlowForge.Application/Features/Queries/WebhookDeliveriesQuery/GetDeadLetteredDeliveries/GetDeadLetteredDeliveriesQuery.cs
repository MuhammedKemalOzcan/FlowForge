using FlowForge.Application.Dtos;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Queries.WebhookDeliveriesQuery.GetDeadLetteredDeliveries;

public record GetDeadLetteredDeliveriesQuery : IRequest<Result<List<WebhookDeliveryDto>>>;
