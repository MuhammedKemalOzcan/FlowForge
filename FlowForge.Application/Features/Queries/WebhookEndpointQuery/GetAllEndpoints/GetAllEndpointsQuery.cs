using FlowForge.Application.Dtos;
using FlowForge.Domain.Entities;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Queries.WebhookEndpointQuery.GetAllEndpoints
{
    public record GetAllEndpointsQuery(Guid TenantId) : IRequest<Result<List<WebhookEndpointDto>>>;
}