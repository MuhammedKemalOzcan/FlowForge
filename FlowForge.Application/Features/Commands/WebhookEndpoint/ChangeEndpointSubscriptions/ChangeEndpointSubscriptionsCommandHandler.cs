using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using FlowForge.Domain.ValueObjects;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointSubscriptions
{
    public class ChangeEndpointSubscriptionsCommandHandler : IRequestHandler<ChangeEndpointSubscriptionsCommand, Result>
    {
        private readonly IWebhookEndpointRepository _webhookEndpointRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeEndpointSubscriptionsCommandHandler(IWebhookEndpointRepository webhookEndpointRepository, IUnitOfWork unitOfWork)
        {
            _webhookEndpointRepository = webhookEndpointRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ChangeEndpointSubscriptionsCommand request, CancellationToken cancellationToken)
        {
            var endpoint = await _webhookEndpointRepository.GetByIdAsync(request.EndpointId, request.TenantId);

            if (endpoint is null) return Result.Failure(DomainErrors.WebhookEndpoint.NotFound);

            var eventTypeResults = request.EventTypes.Select(EventType.Create).ToList();

            var failedEventType = eventTypeResults.FirstOrDefault(r => !r.IsSuccess);
            if (failedEventType is not null) return Result.Failure(failedEventType.Error);

            var eventTypes = eventTypeResults.Select(r => r.Data).ToList();

            endpoint.UpdateSubscription(eventTypes);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}