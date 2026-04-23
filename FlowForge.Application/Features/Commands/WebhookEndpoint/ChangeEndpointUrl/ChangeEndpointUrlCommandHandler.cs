using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using FlowForge.Domain.ValueObjects;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointUrl
{
    public class ChangeEndpointUrlCommandHandler : IRequestHandler<ChangeEndpointUrlCommand, Result>
    {
        private readonly IWebhookEndpointRepository _webhookEndpointRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeEndpointUrlCommandHandler(IWebhookEndpointRepository webhookEndpointRepository, IUnitOfWork unitOfWork)
        {
            _webhookEndpointRepository = webhookEndpointRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ChangeEndpointUrlCommand request, CancellationToken cancellationToken)
        {
            var endpoint = await _webhookEndpointRepository.GetByIdAsync(request.EndpointId, request.TenantId);

            if (endpoint is null) return Result.Failure(DomainErrors.WebhookEndpoint.NotFound);

            var newUrl = Url.Create(request.Url);
            if (!newUrl.IsSuccess) return Result.Failure(newUrl.Error);

            endpoint.ChangeTargetUrl(newUrl.Data);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}