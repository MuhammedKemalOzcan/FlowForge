using FlowForge.Application.Abstractions;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.ActivateEndpoint;

public class ActivateEndpointCommandHandler : IRequestHandler<ActivateEndpointCommand, Result>
{
    private readonly IWebhookEndpointRepository _endpointRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public ActivateEndpointCommandHandler(
        IWebhookEndpointRepository endpointRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _endpointRepository = endpointRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result> Handle(ActivateEndpointCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.GetRequiredTenantId();

        var endpoint = await _endpointRepository.GetByIdAsync(request.Id, tenantId, cancellationToken);

        if (endpoint is null) return Result.Failure(DomainErrors.WebhookEndpoint.NotFound);

        endpoint.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
