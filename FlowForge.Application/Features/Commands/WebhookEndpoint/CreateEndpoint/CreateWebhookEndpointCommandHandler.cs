using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using FlowForge.Domain.ValueObjects;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.CreateEndpoint
{
    public class CreateWebhookEndpointCommandHandler : IRequestHandler<CreateWebhookEndpointCommand, Result<Guid>>
    {
        private readonly IWebhookEndpointRepository _endpointRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWebhookEndpointCommandHandler(IWebhookEndpointRepository endpointRepository, IUnitOfWork unitOfWork)
        {
            _endpointRepository = endpointRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateWebhookEndpointCommand request, CancellationToken cancellationToken)
        {
            var endpointName = EndpointName.Create(request.EndpointName);
            if (!endpointName.IsSuccess) return Result<Guid>.Failure(endpointName.Error);

            var url = Url.Create(request.TargetUrl);
            if (!url.IsSuccess) return Result<Guid>.Failure(url.Error);

            var eventTypeResults = request.EventTypes.Select(EventType.Create).ToList();

            //Herhangi biri başarısızsa ilk hatayı dön.
            var failedEventType = eventTypeResults.FirstOrDefault(r => !r.IsSuccess);
            if (failedEventType is not null) return Result<Guid>.Failure(failedEventType.Error);

            // Hepsi başarılıysa value'ları çıkar
            var eventTypes = eventTypeResults.Select(r => r.Data).ToList();

            var secret = SigningSecret.Create();

            var retryPolicy = RetryPolicy.Create(
                request.MaxAttempts,
                request.Strategy,
                request.InitialDelay,
                request.MaxDelay,
                request.Timeout
                );
            if (!retryPolicy.IsSuccess) return Result<Guid>.Failure(retryPolicy.Error);

            var endpoint = Domain.Entities.WebhookEndpoint.Create(
                request.TenantId,
                endpointName.Data,
                url.Data,
                eventTypes,
                secret,
                retryPolicy.Data
                );

            _endpointRepository.Add(endpoint);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(endpoint.Id);
        }
    }
}