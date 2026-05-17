using FlowForge.Application.Dtos;
using FlowForge.Domain.Entities;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Queries.WebhookEndpointQuery.GetAllEndpoints
{
    public record GetAllEndpointsQuery() : IRequest<Result<List<WebhookEndpointDto>>>;
}