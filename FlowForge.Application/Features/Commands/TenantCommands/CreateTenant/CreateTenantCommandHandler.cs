using FlowForge.Domain.Entities;
using FlowForge.Domain.Errors;
using FlowForge.Domain.Repositories;
using MediatR;

namespace FlowForge.Application.Features.Commands.TenantCommands.CreateTenant
{
    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTenantCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            var tenant = Tenant.Create(request.Name, request.UserId);
            if (!tenant.IsSuccess) return Result<Guid>.Failure(tenant.Error);

            _tenantRepository.Add(tenant.Data);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(tenant.Data.Id);
        }
    }
}