using FlowForge.Application.Dtos;
using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Queries.WebhookDeliveriesQuery.GetAllDeliveries
{
    public record GetAllDeliveriesQuery(int Page = 1, int PageSize = 20, DeliveryStatus? Status = null)
        : IRequest<Result<PagedResultDto<WebhookDeliveryDto>>>;
}
