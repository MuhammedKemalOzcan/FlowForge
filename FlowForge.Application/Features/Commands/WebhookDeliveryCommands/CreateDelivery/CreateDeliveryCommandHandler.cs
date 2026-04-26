using FlowForge.Application.Messages;
using FlowForge.Domain.Entities;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
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

        public CreateDeliveryCommandHandler(IWebhookDeliveryRepository webhookDelivery, IUnitOfWork unitOfWork, IWebhookEndpointRepository endpointRepository, IPublishEndpoint publishEndpoint)
        {
            _webhookDelivery = webhookDelivery;
            _unitOfWork = unitOfWork;
            _endpointRepository = endpointRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<Guid>> Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
        {
            var existingDelivery = await _webhookDelivery.GetByIdempotencyKey(request.IdempotencyKey, request.TenantId);
            if (existingDelivery is not null) return Result<Guid>.Success(existingDelivery.Id);

            var eventType = EventType.Create(request.EventType);
            if (!eventType.IsSuccess) return Result<Guid>.Failure(eventType.Error);

            var idempotencyKey = IdempotencyKey.Create(request.IdempotencyKey);
            if (!idempotencyKey.IsSuccess) return Result<Guid>.Failure(idempotencyKey.Error);

            var endpoint = await _endpointRepository.GetByIdAsync(request.EndpointId, request.TenantId);

            if (endpoint is null) return Result<Guid>.Failure(DomainErrors.WebhookEndpoint.NotFound);
            if (!endpoint.IsActive) return Result<Guid>.Failure(DomainErrors.WebhookEndpoint.EndpointNotActive);
            if (!endpoint.SubscribedEventTypes.Contains(eventType.Data)) return Result<Guid>.Failure(DomainErrors.WebhookEndpoint.NotSubscribedToEventType);

            var delivery = WebhookDelivery.Create(
                    request.TenantId,
                    request.EndpointId,
                    eventType.Data,
                    request.Payload,
                    idempotencyKey.Data,
                    endpoint.RetryPolicy
                    );

            _webhookDelivery.Add(delivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new ProcessWebhookDeliveryMessage(delivery.Id, delivery.TenantId), cancellationToken);

            return Result<Guid>.Success(delivery.Id);
        }
    }
}