using FlowForge.Domain.Enums;

namespace FlowForge.Domain.Entities
{
    public class Membership
    {
        public Guid UserId { get; private set; }
        public Roles Role { get; private set; }
        public DateTime JoinedAt { get; private set; }

        private Membership()
        { }

        internal Membership(Guid userId, Roles roles)
        {
            if (userId == Guid.Empty) throw new ArgumentException("user cannot be found!");
            UserId = userId;
            Role = roles;
            JoinedAt = DateTime.UtcNow;
        }

        internal void ChangeRole(Roles newRole)
        {
            Role = newRole;
        }
    }
}