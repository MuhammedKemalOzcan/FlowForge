using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.RemoveEndpoint
{
    public class RemoveEndpointCommandHandler : IRequestHandler<RemoveEndpointCommand, Result>
    {
        private readonly IWebhookEndpointRepository _endpointRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveEndpointCommandHandler(IWebhookEndpointRepository endpointRepository, IUnitOfWork unitOfWork)
        {
            _endpointRepository = endpointRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(RemoveEndpointCommand request, CancellationToken cancellationToken)
        {
            var endpoint = await _endpointRepository.GetByIdAsync(request.Id, request.tenantId);

            if (endpoint is null) return Result.Failure(DomainErrors.WebhookEndpoint.NotFound);

            endpoint.Deactivate();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}