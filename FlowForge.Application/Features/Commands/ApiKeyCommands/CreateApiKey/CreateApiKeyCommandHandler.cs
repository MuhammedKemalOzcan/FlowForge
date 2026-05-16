using FlowForge.Application.Dtos;
using FlowForge.Domain.Entities;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using MediatR;

namespace FlowForge.Application.Features.Commands.ApiKeyCommands.CreateApiKey
{
    public class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, Result<ApiKeyCreationResultDto>>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateApiKeyCommandHandler(ITenantRepository tenantRepository, IApiKeyRepository apiKeyRepository, IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _apiKeyRepository = apiKeyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ApiKeyCreationResultDto>> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantRepository.GetById(request.TenantId);

            if (tenant is null)
                return Result<ApiKeyCreationResultDto>.Failure(DomainErrors.Tenant.TenantCannotFound);

            var apiKeyCreation = ApiKey.Create(tenant.Id, request.Name);

            _apiKeyRepository.Add(apiKeyCreation.ApiKey);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //İçerisinde plain text key olduğundan bu Dto kesinlikle loglanmamalı.
            var resultDto = new ApiKeyCreationResultDto
            {
                ApiKeyId = apiKeyCreation.ApiKey.Id,
                Name = apiKeyCreation.ApiKey.Name,
                PlainTextKey = apiKeyCreation.RawKey
            };

            return Result<ApiKeyCreationResultDto>.Success(resultDto);
        }
    }
}