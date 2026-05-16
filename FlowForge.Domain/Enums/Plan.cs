using FlowForge.Domain.ValueObjects;

namespace FlowForge.Domain.Enums
{
    public enum Plan
    {
        Free,
        Starter,
        Pro,
        Enterprise
    }

    public static class PlanExtensions
    {
        public static PlanLimits GetLimits(this Plan plan)
            => plan switch
            {
                Plan.Free => PlanLimits.Create(1, 3, 1),
                Plan.Starter => PlanLimits.Create(5, 100, 3),
                Plan.Pro => PlanLimits.Create(25, 1000, 10),
                Plan.Enterprise => PlanLimits.Create(50, 10000, 50),
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}



