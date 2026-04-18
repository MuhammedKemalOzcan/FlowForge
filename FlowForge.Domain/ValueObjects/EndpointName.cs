using FlowForge.Domain.Errors;

namespace FlowForge.Domain.ValueObjects
{
    public record EndpointName
    {
        public string Value { get; private set; }

        private EndpointName(string value)
        {
            Value = value;
        }

        public static Result<EndpointName> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result<EndpointName>.Failure(DomainErrors.EndpointName.NameEmpty);
            value = value.Trim();
            if (value.Length > 100 || value.Length < 2)
                return Result<EndpointName>.Failure(DomainErrors.EndpointName.InvalidNameLength);

            return Result<EndpointName>.Success(new EndpointName(value));
        }
    }
}