using FlowForge.Application.Abstractions;
using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookDeliveryCommands.RequeueDeadLetteredDelivery;

public class RequeueDeadLetteredDeliveryCommandHandler : IRequestHandler<RequeueDeadLetteredDeliveryCommand, Result>
{
    private readonly IWebhookDeliveryRepository _deliveryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public RequeueDeadLetteredDeliveryCommandHandler(
        IWebhookDeliveryRepository deliveryRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _deliveryRepository = deliveryRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result> Handle(RequeueDeadLetteredDeliveryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.GetRequiredTenantId();

        var delivery = await _deliveryRepository.GetByIdAsync(request.DeliveryId, tenantId, cancellationToken);

        if (delivery is null) return Result.Failure(DomainErrors.WebhookDelivery.DeliveryNotFound);

        if (delivery.Status != DeliveryStatus.DeadLettered)
            return Result.Failure(DomainErrors.WebhookDelivery.NotDeadLettered);

        delivery.RequeueFromDeadLetter();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
