using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookDeliveryCommands.RequeueDeadLetteredDelivery;

public record RequeueDeadLetteredDeliveryCommand(Guid DeliveryId) : IRequest<Result>;
