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

        public WebhookDeliveryQueryHandler(IFlowForgeApiDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<WebhookDeliveryDto>>> Handle(WebhookDeliveryQuery request, CancellationToken cancellationToken)
        {
            var delivery = await _context.WebhookDeliveries
                .AsNoTracking()
                .Where(x => x.TenantId == request.TenantId)
                .Select(x => new WebhookDeliveryDto
                {
                    Id = x.Id,
                    EventType = x.EventType.Value,
                    Payload = x.Payload,
                    Status = x.Status,
                    RetryPolicy = new RetryPolicyDto
                    {
                        MaxAttempts = x.RetryPolicy.MaxAttempts,
                        InitialDelay = x.RetryPolicy.InitialDelay,
                        MaxDelay = x.RetryPolicy.MaxDelay,
                        Strategy = x.RetryPolicy.Strategy,
                        Timeout = x.RetryPolicy.TimeOut
                    },
                    ReceivedAt = x.ReceivedAt,
                    NextRetryAt = x.NextRetryAt,
                    FinalResultAt = x.FinalResultAt,
                    DeliveryAttempts = x.Attempts.Select(a => new DeliveryAttemptDto
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

            return Result<List<WebhookDeliveryDto>>.Success(delivery);
        }
    }
}