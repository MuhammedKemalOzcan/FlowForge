using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using FlowForge.Domain.ValueObjects;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointName
{
    public class ChangeEndpointNameCommandHandler : IRequestHandler<ChangeEndpointNameCommand, Result>
    {
        private readonly IWebhookEndpointRepository _webhookEndpointRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeEndpointNameCommandHandler(IWebhookEndpointRepository webhookEndpointRepository, IUnitOfWork unitOfWork)
        {
            _webhookEndpointRepository = webhookEndpointRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ChangeEndpointNameCommand request, CancellationToken cancellationToken)
        {
            var endpoint = await _webhookEndpointRepository.GetByIdAsync(request.EndpointId, request.TenantId);

            if (endpoint is null) return Result.Failure(DomainErrors.WebhookEndpoint.NotFound);

            var newName = EndpointName.Create(request.EndpointName);
            if (!newName.IsSuccess) return Result.Failure(newName.Error);

            endpoint.ChangeName(newName.Data);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}