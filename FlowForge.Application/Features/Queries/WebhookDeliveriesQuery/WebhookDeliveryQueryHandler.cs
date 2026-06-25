using FlowForge.Application.Abstractions;
using FlowForge.Application.Data;
using FlowForge.Application.Dtos;
using FlowForge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Application.Features.Queries.WebhookDeliveriesQuery
{
    public class WebhookDeliveryQueryHandler : IRequestHandler<WebhookDeliveryQuery, Result<List<WebhookDeliveryDto>>>
    {
        private readonly IFlowForgeApiDbContext _context;
        private readonly ICurrentTenant _currentTenant;

        public WebhookDeliveryQueryHandler(IFlowForgeApiDbContext context, ICurrentTenant currentTenant)
        {
            _context = context;
            _currentTenant = currentTenant;
        }

        public async Task<Result<List<WebhookDeliveryDto>>> Handle(WebhookDeliveryQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _currentTenant.GetRequiredTenantId();

            var deliveries = await (
             from delivery in _context.WebhookDeliveries.AsNoTracking()
             join endpoint in _context.WebhookEndpoints.AsNoTracking()
                 on delivery.EndpointId equals endpoint.Id
             where delivery.TenantId == tenantId
                && endpoint.TenantId == tenantId
                && (request.EndpointId == null || delivery.EndpointId == request.EndpointId)
             select new WebhookDeliveryDto
             {
                 Id = delivery.Id,
                 EndpointName = endpoint.Name.Value,
                 EventType = delivery.EventType.Value,
                 Payload = delivery.Payload,
                 Status = delivery.Status,
                 RetryPolicy = new RetryPolicyDto
                 {
                     MaxAttempts = delivery.RetryPolicy.MaxAttempts,
                     InitialDelay = delivery.RetryPolicy.InitialDelay,
                     MaxDelay = delivery.RetryPolicy.MaxDelay,
                     Strategy = delivery.RetryPolicy.Strategy,
                     Timeout = delivery.RetryPolicy.TimeOut
                 },

                 ReceivedAt = delivery.ReceivedAt,
                 NextRetryAt = delivery.NextRetryAt,
                 FinalResultAt = delivery.FinalResultAt,

                 DeliveryAttempts = delivery.Attempts
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
             }).ToListAsync(cancellationToken);

            return Result<List<WebhookDeliveryDto>>.Success(deliveries);
        }
    }
}