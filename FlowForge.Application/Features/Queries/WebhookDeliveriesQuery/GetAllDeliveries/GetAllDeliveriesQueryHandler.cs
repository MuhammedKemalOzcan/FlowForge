using FlowForge.Application.Abstractions;
using FlowForge.Application.Data;
using FlowForge.Application.Dtos;
using FlowForge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Application.Features.Queries.WebhookDeliveriesQuery.GetAllDeliveries
{
    public class GetAllDeliveriesQueryHandler : IRequestHandler<GetAllDeliveriesQuery, Result<PagedResultDto<WebhookDeliveryDto>>>
    {
        private readonly IFlowForgeApiDbContext _context;
        private readonly ICurrentTenant _currentTenant;

        private const int MaxPageSize = 100;

        public GetAllDeliveriesQueryHandler(IFlowForgeApiDbContext context, ICurrentTenant currentTenant)
        {
            _context = context;
            _currentTenant = currentTenant;
        }

        public async Task<Result<PagedResultDto<WebhookDeliveryDto>>> Handle(GetAllDeliveriesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _currentTenant.GetRequiredTenantId();

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, MaxPageSize);

            var baseQuery =
                from delivery in _context.WebhookDeliveries.AsNoTracking()
                join endpoint in _context.WebhookEndpoints.AsNoTracking()
                    on delivery.EndpointId equals endpoint.Id
                where delivery.TenantId == tenantId
                   && endpoint.TenantId == tenantId
                   && (request.Status == null || delivery.Status == request.Status)
                select new { delivery, endpoint };

            var totalCount = await baseQuery.CountAsync(cancellationToken);

            var items = await baseQuery
                .OrderByDescending(x => x.delivery.ReceivedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WebhookDeliveryDto
                {
                    Id = x.delivery.Id,
                    EndpointName = x.endpoint.Name.Value,
                    EventType = x.delivery.EventType.Value,
                    Payload = x.delivery.Payload,
                    Status = x.delivery.Status,
                    RetryPolicy = new RetryPolicyDto
                    {
                        MaxAttempts = x.delivery.RetryPolicy.MaxAttempts,
                        InitialDelay = x.delivery.RetryPolicy.InitialDelay,
                        MaxDelay = x.delivery.RetryPolicy.MaxDelay,
                        Strategy = x.delivery.RetryPolicy.Strategy,
                        Timeout = x.delivery.RetryPolicy.TimeOut
                    },
                    ReceivedAt = x.delivery.ReceivedAt,
                    NextRetryAt = x.delivery.NextRetryAt,
                    FinalResultAt = x.delivery.FinalResultAt,
                    DeliveryAttempts = x.delivery.Attempts
                        .OrderBy(a => a.AttemptNumber)
                        .Select(a => new DeliveryAttemptDto
                        {
                            Id = a.Id,
                            AttemptNumber = a.AttemptNumber,
                            StartedAt = a.StartedAt,
                            CompletedAt = a.CompletedAt,
                            DurationMs = a.DurationMs,
                            StatusCode = a.StatusCode,
                            ResponseBody = a.ResponseBodySnippet,
                            ErrorMessage = a.ErrorMessage,
                            OutcomeStatus = a.Outcome
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return Result<PagedResultDto<WebhookDeliveryDto>>.Success(new PagedResultDto<WebhookDeliveryDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }
    }
}
