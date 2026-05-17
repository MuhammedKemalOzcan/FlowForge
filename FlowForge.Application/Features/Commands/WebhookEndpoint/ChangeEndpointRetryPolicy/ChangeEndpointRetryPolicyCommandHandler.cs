using FlowForge.Application.Abstractions;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using FlowForge.Domain.ValueObjects;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointRetryPolicy
{
    public class ChangeEndpointRetryPolicyCommandHandler : IRequestHandler<ChangeEndpointRetryPolicyCommand, Result>
    {
        private readonly IWebhookEndpointRepository _endpointRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTenant _currentTenant;

        public ChangeEndpointRetryPolicyCommandHandler(IWebhookEndpointRepository endpointRepository, IUnitOfWork unitOfWork, ICurrentTenant currentTenant)
        {
            _endpointRepository = endpointRepository;
            _unitOfWork = unitOfWork;
            _currentTenant = currentTenant;
        }

        public async Task<Result> Handle(ChangeEndpointRetryPolicyCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _currentTenant.GetRequiredTenantId();

            var endpoint = await _endpointRepository.GetByIdAsync(request.EndpointId, tenantId);

            if (endpoint is null) return Result.Failure(DomainErrors.WebhookEndpoint.NotFound);

            var newRetryPolicy = RetryPolicy.Create(
                request.MaxAttempts,
                request.Strategy,
                request.InitialDelay,
                request.MaxDelay,
                request.TimeOut
                );
            if (!newRetryPolicy.IsSuccess) return Result.Failure(newRetryPolicy.Error);

            endpoint.ChangeRetryPolicy(newRetryPolicy.Data);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}