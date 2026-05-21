using FlowForge.Application.Abstractions;
using FlowForge.Application.Data;
using FlowForge.Application.Dtos;
using FlowForge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Application.Features.Queries.WebhookDeliveriesQuery.GetDeliveryById;

public class GetDeliveryByIdQueryHandler : IRequestHandler<GetDeliveryByIdQuery, Result<WebhookDeliveryDto>>
{
    private readonly IFlowForgeApiDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public GetDeliveryByIdQueryHandler(IFlowForgeApiDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<Result<WebhookDeliveryDto>> Handle(GetDeliveryByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.GetRequiredTenantId();

        var delivery = await _context.WebhookDeliveries
            .AsNoTracking()
            .Where(x => x.Id == request.DeliveryId && x.TenantId == tenantId)
            .Select(x => new WebhookDeliveryDto
            {
                Id            = x.Id,
                EventType     = x.EventType.Value,
                Payload       = x.Payload,
                Status        = x.Status,
                RetryPolicy   = new RetryPolicyDto
                {
                    MaxAttempts  = x.RetryPolicy.MaxAttempts,
                    InitialDelay = x.RetryPolicy.InitialDelay,
                    MaxDelay     = x.RetryPolicy.MaxDelay,
                    Strategy     = x.RetryPolicy.Strategy,
                    Timeout      = x.RetryPolicy.TimeOut
                },
                ReceivedAt       = x.ReceivedAt,
                NextRetryAt      = x.NextRetryAt,
                FinalResultAt    = x.FinalResultAt,
                DeliveryAttempts = x.Attempts.Select(a => new DeliveryAttemptDto
                {
                    Id            = a.Id,
                    AttemptNumber = a.AttemptNumber,
                    StartedAt     = a.StartedAt,
                    CompletedAt   = a.CompletedAt,
                    DurationMs    = a.DurationMs,
                    StatusCode    = a.StatusCode,
                    ResponseBody  = a.ResponseBodySnippet,
                    ErrorMessage  = a.ErrorMessage,
                    OutcomeStatus = a.Outcome
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (delivery is null)
            return Result<WebhookDeliveryDto>.Failure(DomainErrors.WebhookDelivery.DeliveryNotFound);

        return Result<WebhookDeliveryDto>.Success(delivery);
    }
}
