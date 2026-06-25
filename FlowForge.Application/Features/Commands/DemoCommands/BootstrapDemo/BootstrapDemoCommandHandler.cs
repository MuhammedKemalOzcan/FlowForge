using FlowForge.Application.Dtos;
using FlowForge.Domain.Entities;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using FlowForge.Domain.ValueObjects;
using MediatR;

namespace FlowForge.Application.Features.Commands.DemoCommands.BootstrapDemo
{
    public class BootstrapDemoCommandHandler : IRequestHandler<BootstrapDemoCommand, Result<DemoBootstrapResultDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BootstrapDemoCommandHandler(
            IUserRepository userRepository,
            ITenantRepository tenantRepository,
            IApiKeyRepository apiKeyRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _apiKeyRepository = apiKeyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DemoBootstrapResultDto>> Handle(BootstrapDemoCommand request, CancellationToken cancellationToken)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8];

            var externalIdResult = ExternalIdentityId.Create($"demo_{suffix}");
            if (!externalIdResult.IsSuccess)
                return Result<DemoBootstrapResultDto>.Failure(externalIdResult.Error);

            var emailResult = Email.Create($"demo_{suffix}@flowforge-demo.com");
            if (!emailResult.IsSuccess)
                return Result<DemoBootstrapResultDto>.Failure(emailResult.Error);

            var user = User.CreateFromIdentityProvider(externalIdResult.Data!, emailResult.Data!, "Demo User");
            _userRepository.Add(user);

            var tenantResult = Tenant.CreateDemo("Demo Workspace", user.Id);
            if (!tenantResult.IsSuccess)
                return Result<DemoBootstrapResultDto>.Failure(tenantResult.Error);

            _tenantRepository.Add(tenantResult.Data!);

            var apiKeyCreation = ApiKey.Create(tenantResult.Data!.Id, "Demo API Key");
            _apiKeyRepository.Add(apiKeyCreation.ApiKey);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<DemoBootstrapResultDto>.Success(new DemoBootstrapResultDto
            {
                TenantId = tenantResult.Data.Id,
                ApiKey = apiKeyCreation.RawKey,
                ExpiresAt = tenantResult.Data.DemoExpiresAt!.Value
            });
        }
    }
}
