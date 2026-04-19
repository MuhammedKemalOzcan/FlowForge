namespace FlowForge.Domain.ValueObjects
{
    public record PlanLimits
    {
        public int MaxEndpointsAllowed { get; private set; }
        public int MaxEventsPerMinute { get; private set; }
        public int MaxMembersAllowed { get; private set; }

        private PlanLimits() { }

        private PlanLimits(int endpoints, int events, int allowedMembers)
        {
            MaxEndpointsAllowed = endpoints;
            MaxEventsPerMinute = events;
            MaxMembersAllowed = allowedMembers;
        }

        public static PlanLimits Create(int endpoints, int events, int allowedMembers)
        {
            if (endpoints < 0) throw new ArgumentOutOfRangeException(nameof(endpoints), "cannot be negative!");
            if (events < 0) throw new ArgumentOutOfRangeException(nameof(events), "cannot be negative!");
            if (allowedMembers < 0) throw new ArgumentOutOfRangeException(nameof(allowedMembers), "cannot be negative!");
            return new PlanLimits(endpoints, events, allowedMembers);
        }
    }
}