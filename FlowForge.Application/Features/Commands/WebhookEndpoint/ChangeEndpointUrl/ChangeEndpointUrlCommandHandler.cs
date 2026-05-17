using FlowForge.Application.Abstractions;
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
        private readonly ICurrentTenant _currentTenant;

        public ChangeEndpointUrlCommandHandler(IWebhookEndpointRepository webhookEndpointRepository, IUnitOfWork unitOfWork, ICurrentTenant currentTenant)
        {
            _webhookEndpointRepository = webhookEndpointRepository;
            _unitOfWork = unitOfWork;
            _currentTenant = currentTenant;
        }

        public async Task<Result> Handle(ChangeEndpointUrlCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _currentTenant.GetRequiredTenantId();

            var endpoint = await _webhookEndpointRepository.GetByIdAsync(request.EndpointId, tenantId);

            if (endpoint is null) return Result.Failure(DomainErrors.WebhookEndpoint.NotFound);

            var newUrl = Url.Create(request.Url);
            if (!newUrl.IsSuccess) return Result.Failure(newUrl.Error);

            endpoint.ChangeTargetUrl(newUrl.Data);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}