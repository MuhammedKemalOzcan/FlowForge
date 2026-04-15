using FlowForge.Domain.Errors;

namespace FlowForge.Domain.ValueObjects
{
    public record IdempotencyKey
    {
        public string Value { get; init; }

        private IdempotencyKey(string value)
        {
            Value = value;
        }

        public static Result<IdempotencyKey> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result<IdempotencyKey>.Failure(DomainErrors.IdempotencyKey.Empty);
            if (value.Length < 1 || value.Length > 255)
                return Result<IdempotencyKey>.Failure(DomainErrors.IdempotencyKey.InvalidLength);
            if (!value.All(x => x >= 32 && x <= 126))
                return Result<IdempotencyKey>.Failure(DomainErrors.IdempotencyKey.InvalidFormat);

            return Result<IdempotencyKey>.Success(new IdempotencyKey(value));
        }
    }
}