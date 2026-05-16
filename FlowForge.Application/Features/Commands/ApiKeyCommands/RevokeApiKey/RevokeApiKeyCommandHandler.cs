using FlowForge.Application.Abstractions;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using MediatR;

namespace FlowForge.Application.Features.Commands.ApiKeyCommands.RevokeApiKey
{
    public class RevokeApiKeyCommandHandler : IRequestHandler<RevokeApiKeyCommand, Result>
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApiKeyValidationCache _validationCache;

        public RevokeApiKeyCommandHandler(IApiKeyRepository apiKeyRepository, IUnitOfWork unitOfWork, IApiKeyValidationCache validationCache)
        {
            _apiKeyRepository = apiKeyRepository;
            _unitOfWork = unitOfWork;
            _validationCache = validationCache;
        }

        public async Task<Result> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
        {
            var apiKey = await _apiKeyRepository.GetByIdForTenant(request.ApiKeyId, request.TenantId, cancellationToken);

            if (apiKey is null)
                return Result.Failure(DomainErrors.ApiKey.NotFound);

            var revokeResult = apiKey.Revoke();

            if (!revokeResult.IsSuccess)
                return Result.Failure(revokeResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _validationCache.RemoveAsync(apiKey.Key);

            return Result.Success();
        }
    }
}