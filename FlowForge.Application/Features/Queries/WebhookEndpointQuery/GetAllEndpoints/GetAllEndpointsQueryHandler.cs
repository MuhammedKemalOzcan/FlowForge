using FlowForge.Application.Abstractions;
using FlowForge.Application.Data;
using FlowForge.Application.Dtos;
using FlowForge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Application.Features.Queries.WebhookEndpointQuery.GetAllEndpoints
{
    public class GetAllEndpointsQueryHandler : IRequestHandler<GetAllEndpointsQuery, Result<List<WebhookEndpointDto>>>
    {
        private readonly IFlowForgeApiDbContext _context;
        private readonly ICurrentTenant _currentTenant;

        public GetAllEndpointsQueryHandler(IFlowForgeApiDbContext context, ICurrentTenant currentTenant)
        {
            _context = context;
            _currentTenant = currentTenant;
        }

        //TODO: Opsiyonel filtre kullanarak Deactive olmuş endpointleri gösterip göstermemeyi hallet
        public async Task<Result<List<WebhookEndpointDto>>> Handle(GetAllEndpointsQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _currentTenant.GetRequiredTenantId();

            var endpoints = await _context.WebhookEndpoints
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .Select(x => new WebhookEndpointDto
                {
                    Id = x.Id,
                    Name = x.Name.Value,
                    TargetUrl = x.TargetUrl.Value,
                    EventTypes = x.SubscribedEventTypes.Select(e => e.Value).ToList(),
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    RetryPolicy = new RetryPolicyDto
                    {
                        MaxAttempts = x.RetryPolicy.MaxAttempts,
                        InitialDelay = x.RetryPolicy.InitialDelay,
                        MaxDelay = x.RetryPolicy.MaxDelay,
                        Strategy = x.RetryPolicy.Strategy,
                        Timeout = x.RetryPolicy.TimeOut
                    }
                })
                .ToListAsync();

            return Result<List<WebhookEndpointDto>>.Success(endpoints);
        }
    }
}