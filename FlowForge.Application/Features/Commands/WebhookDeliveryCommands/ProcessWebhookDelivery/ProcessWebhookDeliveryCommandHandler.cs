using FlowForge.Application.Abstractions;
using FlowForge.Application.Streaming;
using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using FlowForge.Domain.Services;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookDeliveryCommands.ProcessWebhookDelivery
{
    public class ProcessWebhookDeliveryCommandHandler : IRequestHandler<ProcessWebhookDeliveryCommand, Result>
    {
        private readonly IWebhookDeliveryRepository _deliveryRepository;
        private readonly IWebhookEndpointRepository _endpointRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebhookSender _webhookSender;
        private readonly IWebhookDeliveryStreamBroadcaster _streamBroadcaster;

        public ProcessWebhookDeliveryCommandHandler(IWebhookDeliveryRepository deliveryRepository, IWebhookEndpointRepository endpointRepository, IUnitOfWork unitOfWork, IWebhookSender webhookSender, IWebhookDeliveryStreamBroadcaster streamBroadcaster)
        {
            _deliveryRepository = deliveryRepository;
            _endpointRepository = endpointRepository;
            _unitOfWork = unitOfWork;
            _webhookSender = webhookSender;
            _streamBroadcaster = streamBroadcaster;
        }

        public async Task<Result> Handle(ProcessWebhookDeliveryCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(request.DeliveryId, request.TenantId);
            if (delivery is null) return Result.Failure(DomainErrors.WebhookDelivery.DeliveryNotFound);

            if (delivery.Status != DeliveryStatus.Queued) return Result.Failure(DomainErrors.WebhookDelivery.NotInQueuedState);

            var endpoint = await _endpointRepository.GetByIdAsync(delivery.EndpointId, request.TenantId,cancellationToken);
            if (endpoint is null) return Result.Failure(DomainErrors.WebhookEndpoint.NotFound);

            
            delivery.MarkInProgress();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _streamBroadcaster.PublishAsync(WebhookDeliveryStreamEvent.From(delivery, "in_progress"), cancellationToken);

            var signature = endpoint.SigningSecret.ComputeSignature(delivery.Payload);

            var result = await _webhookSender.SendAsync(
                endpoint.TargetUrl.Value,
                delivery.Payload, signature,
                delivery.EventType.Value,
                delivery.Id,
                cancellationToken);

            if (result.IsSuccess)
            {
                delivery.RecordSuccessfulAttempt(result.DurationMs, result.StatusCode, result.ResponseBodySnippet, result.StartedAt, result.CompletedAt);
            }
            else
            {
                delivery.RecordFailedAttempt(result.DurationMs, result.StatusCode, result.ErrorMessage, result.StartedAt, result.CompletedAt);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var eventName = delivery.Status switch
            {
                DeliveryStatus.Succeeded => "succeeded",
                DeliveryStatus.DeadLettered => "dead_lettered",
                _ => "retry_scheduled"
            };
            await _streamBroadcaster.PublishAsync(WebhookDeliveryStreamEvent.From(delivery, eventName), cancellationToken);

            return Result.Success();
        }
    }
}