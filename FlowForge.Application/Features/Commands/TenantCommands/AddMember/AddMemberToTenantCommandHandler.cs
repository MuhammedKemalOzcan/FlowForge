using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using MediatR;

namespace FlowForge.Application.Features.Commands.TenantCommands.AddMember
{
    public class AddMemberToTenantCommandHandler : IRequestHandler<AddMemberToTenantCommand, Result>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddMemberToTenantCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AddMemberToTenantCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantRepository.GetById(request.TenantId);
            if (tenant is null) return Result.Failure(DomainErrors.Tenant.TenantCannotFound);

            var result = tenant.AddMember(request.UserId, request.Role);
            if (!result.IsSuccess) return Result.Failure(result.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}