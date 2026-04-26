using FlowForge.Application.Features.Commands.WebhookDeliveryCommands.ProcessWebhookDelivery;
using FlowForge.Application.Messages;
using MassTransit;
using MediatR;

namespace FlowForge.Application.Consumers
{
    public class ProcessWebhookDeliveryConsumer : IConsumer<ProcessWebhookDeliveryMessage>
    {
        private readonly IMediator _mediator;

        public ProcessWebhookDeliveryConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<ProcessWebhookDeliveryMessage> context)
        {
            var message = context.Message;
            var command = new ProcessWebhookDeliveryCommand(message.DeliveryId, message.TenantId);
            await _mediator.Send(command);
        }
    }
}