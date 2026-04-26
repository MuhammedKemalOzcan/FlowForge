using FlowForge.Application.Messages;
using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using FlowForge.Domain.Services;
using MassTransit;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookDeliveryCommands.ProcessWebhookDelivery
{
    public class ProcessWebhookDeliveryCommandHandler : IRequestHandler<ProcessWebhookDeliveryCommand, Result>
    {
        private readonly IWebhookDeliveryRepository _deliveryRepository;
        private readonly IWebhookEndpointRepository _endpointRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebhookSender _webhookSender;

        public ProcessWebhookDeliveryCommandHandler(IWebhookDeliveryRepository deliveryRepository, IWebhookEndpointRepository endpointRepository, IUnitOfWork unitOfWork, IWebhookSender webhookSender)
        {
            _deliveryRepository = deliveryRepository;
            _endpointRepository = endpointRepository;
            _unitOfWork = unitOfWork;
            _webhookSender = webhookSender;
        }

        public async Task<Result> Handle(ProcessWebhookDeliveryCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(request.DeliveryId, request.TenantId);
            if (delivery is null) return Result.Failure(DomainErrors.WebhookDelivery.DeliveryNotFound);

            var endpoint = await _endpointRepository.GetByIdAsync(delivery.EndpointId, request.TenantId);
            if (endpoint is null) return Result.Failure(DomainErrors.WebhookEndpoint.NotFound);

            if (delivery.Status != DeliveryStatus.Pending) return Result.Failure(DomainErrors.WebhookDelivery.NotInPendingState);

            delivery.MarkInProgress();

            var signature = endpoint.SigningSecret.ComputeSignature(delivery.Payload);

            var result = await _webhookSender.SendAsync(
                endpoint.TargetUrl.Value,
                delivery.Payload, signature,
                delivery.EventType.Value,
                delivery.Id);

            if (result.IsSuccess)
            {
                delivery.RecordSuccessfulAttempt(result.DurationMs, result.StatusCode, result.ResponseBodySnippet, result.StartedAt, result.CompletedAt);
            }
            else
            {
                delivery.RecordFailedAttempt(result.DurationMs, result.StatusCode, result.ErrorMessage, result.StartedAt, result.CompletedAt);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}