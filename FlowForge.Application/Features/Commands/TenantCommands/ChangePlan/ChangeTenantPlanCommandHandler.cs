using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using MediatR;

namespace FlowForge.Application.Features.Commands.TenantCommands.ChangePlan
{
    public class ChangeTenantPlanCommandHandler : IRequestHandler<ChangeTenantPlanCommand, Result>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeTenantPlanCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ChangeTenantPlanCommand request, CancellationToken cancellationToken)
        {
            //Down grade senaryoları için kullanıcı silme ve endpoint silme işlemleri yapılacak!
            var tenant = await _tenantRepository.GetById(request.TenantId);
            if (tenant is null) return Result.Failure(DomainErrors.Tenant.TenantCannotFound);

            tenant.ChangePlan(request.newPlan);
            _tenantRepository.Update(tenant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}