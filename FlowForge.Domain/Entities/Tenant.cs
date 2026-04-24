using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using FlowForge.Domain.ValueObjects;

namespace FlowForge.Domain.Entities
{
    public class Tenant
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Plan Plan { get; private set; }
        public Status TenantStatus { get; private set; }
        public DateTime CreatedAt { get; private set; }
        private readonly List<Membership> _memberships = new();
        public IReadOnlyCollection<Membership> Memberships => _memberships.AsReadOnly();
        public PlanLimits PlanLimits { get; private set; }

        private Tenant()
        {
        }

        public static Result<Tenant> Create(string name, Guid userId)
        {
            if (string.IsNullOrEmpty(name)) return Result<Tenant>.Failure(DomainErrors.Tenant.EmptyName);
            if (userId == Guid.Empty) throw new ArgumentException("user cannot be found");

            var tenant = new Tenant()
            {
                Id = Guid.NewGuid(),
                Name = name,
                Plan = Plan.Free,
                TenantStatus = Status.Active,
                CreatedAt = DateTime.UtcNow,
                PlanLimits = Plan.Free.GetLimits()
            };

            var membership = new Membership(userId, Roles.Owner);
            tenant._memberships.Add(membership);

            return Result<Tenant>.Success(tenant);
        }

        //Handler da downgrade durumlarında gerekli kontrolleri sağla (endpointler silinmeli üye sayısı düşürülmeli)
        public void ChangePlan(Plan newPlan)
        {
            if (Plan == newPlan) return;
            Plan = newPlan;
            PlanLimits = newPlan.GetLimits();
        }

        public Result AddMember(Guid userId, Roles role)
        {
            if (userId == Guid.Empty) throw new ArgumentException("user cannot found!");
            if (_memberships.Count >= PlanLimits.MaxMembersAllowed) return Result.Failure(DomainErrors.Tenant.ReachedMaxMember);
            if (_memberships.Any(m => m.UserId == userId)) return Result.Failure(DomainErrors.Tenant.MemberAlreadyExist);

            var newMember = new Membership(userId, role);

            _memberships.Add(newMember);

            return Result.Success();
        }

        public Result RemoveMember(Guid userId)
        {
            var membership = _memberships.FirstOrDefault(m => m.UserId == userId);
            if (membership is null) return Result.Failure(DomainErrors.Tenant.MemberCannotFound);
            if (membership.Role == Roles.Owner && _memberships.Count(m => m.Role == Roles.Owner) == 1) return Result.Failure(DomainErrors.Tenant.OwnerCannotBeRemoved);

            _memberships.Remove(membership);

            return Result.Success();
        }

        public Result ChangeMemberRole(Guid userId, Roles role)
        {
            var user = _memberships.FirstOrDefault(m => m.UserId == userId);
            if (user is null) return Result.Failure(DomainErrors.Tenant.MemberCannotFound);
            if (user.Role == Roles.Owner && _memberships.Count(m => m.Role == Roles.Owner) == 1) return Result.Failure(DomainErrors.Tenant.OwnerCannotBeRemoved);

            user.ChangeRole(role);

            return Result.Success();
        }
    }
}