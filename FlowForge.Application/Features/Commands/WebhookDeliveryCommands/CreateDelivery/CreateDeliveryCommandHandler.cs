using FlowForge.Application.Abstractions;
using FlowForge.Application.Messages;
using FlowForge.Domain.Entities;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using FlowForge.Domain.Services;
using FlowForge.Domain.ValueObjects;
using MassTransit;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookDeliveryCommands.CreateDelivery
{
    public class CreateDeliveryCommandHandler : IRequestHandler<CreateDeliveryCommand, Result<Guid>>
    {
        private readonly IWebhookDeliveryRepository _webhookDelivery;
        private readonly IWebhookEndpointRepository _endpointRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRateLimiter _rateLimiter;
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentTenant _currentTenant;

        public CreateDeliveryCommandHandler(IWebhookDeliveryRepository webhookDelivery, IUnitOfWork unitOfWork, IWebhookEndpointRepository endpointRepository, IPublishEndpoint publishEndpoint, IRateLimiter rateLimiter, ITenantRepository tenantRepository, ICurrentTenant currentTenant)
        {
            _webhookDelivery = webhookDelivery;
            _unitOfWork = unitOfWork;
            _endpointRepository = endpointRepository;
            _publishEndpoint = publishEndpoint;
            _rateLimiter = rateLimiter;
            _tenantRepository = tenantRepository;
            _currentTenant = currentTenant;
        }

        public async Task<Result<Guid>> Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _currentTenant.GetRequiredTenantId();

            var tenant = await _tenantRepository.GetById(tenantId);
            if (tenant is null) return Result<Guid>.Failure(DomainErrors.Tenant.TenantCannotFound);

            var isAllowed = await _rateLimiter.IsAllowedAsync(tenant.Id, tenant.PlanLimits.MaxEventsPerMinute);
            if (!isAllowed) return Result<Guid>.Failure(DomainErrors.Tenant.RateLimitExceeded);

            var eventType = EventType.Create(request.EventType);
            if (!eventType.IsSuccess) return Result<Guid>.Failure(eventType.Error);

            var idempotencyKey = IdempotencyKey.Create(request.IdempotencyKey);
            if (!idempotencyKey.IsSuccess) return Result<Guid>.Failure(idempotencyKey.Error);

            var existingDelivery = await _webhookDelivery.GetByIdempotencyKey(request.IdempotencyKey, tenantId);
            if (existingDelivery is not null) return Result<Guid>.Success(existingDelivery.Id);

            var endpoint = await _endpointRepository.GetByIdAsync(request.EndpointId, tenantId, cancellationToken);

            if (endpoint is null) return Result<Guid>.Failure(DomainErrors.WebhookEndpoint.NotFound);
            if (!endpoint.IsActive) return Result<Guid>.Failure(DomainErrors.WebhookEndpoint.EndpointNotActive);
            if (!endpoint.SubscribedEventTypes.Contains(eventType.Data)) return Result<Guid>.Failure(DomainErrors.WebhookEndpoint.NotSubscribedToEventType);

            var delivery = WebhookDelivery.Create(
                    tenantId,
                    request.EndpointId,
                    eventType.Data,
                    request.Payload,
                    idempotencyKey.Data,
                    endpoint.RetryPolicy
                    );

            delivery.MarkQueued();

            _webhookDelivery.Add(delivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new ProcessWebhookDeliveryMessage(delivery.Id, delivery.TenantId), cancellationToken);

            return Result<Guid>.Success(delivery.Id);
        }
    }
}