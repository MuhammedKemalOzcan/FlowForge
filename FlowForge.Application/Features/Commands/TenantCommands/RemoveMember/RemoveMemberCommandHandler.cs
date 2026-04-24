using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using MediatR;

namespace FlowForge.Application.Features.Commands.TenantCommands.RemoveMember
{
    public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Result>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveMemberCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantRepository.GetById(request.TenantId);
            if (tenant is null) return Result.Failure(DomainErrors.Tenant.TenantCannotFound);

            var result = tenant.RemoveMember(request.UserId);
            if (!result.IsSuccess) return Result.Failure(result.Error);

            _tenantRepository.Update(tenant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}