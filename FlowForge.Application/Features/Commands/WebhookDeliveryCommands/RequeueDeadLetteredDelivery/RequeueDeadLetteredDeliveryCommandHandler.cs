using FlowForge.Application.Abstractions;
using FlowForge.Application.Streaming;
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
    private readonly IWebhookDeliveryStreamBroadcaster _streamBroadcaster;

    public RequeueDeadLetteredDeliveryCommandHandler(
        IWebhookDeliveryRepository deliveryRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        IWebhookDeliveryStreamBroadcaster streamBroadcaster)
    {
        _deliveryRepository = deliveryRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _streamBroadcaster = streamBroadcaster;
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

        await _streamBroadcaster.PublishAsync(WebhookDeliveryStreamEvent.From(delivery, "requeued"), cancellationToken);

        return Result.Success();
    }
}
