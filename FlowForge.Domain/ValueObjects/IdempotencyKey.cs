namespace FlowForge.Domain.ValueObjects
{
    public record IdempotencyKey
    {
        public string Value { get; init; }

        private IdempotencyKey(string value)
        {
            Value = value;
        }

        public static IdempotencyKey Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Idempotency key value cannot be empty");
            if (value.Length < 1 || value.Length > 255)
                throw new ArgumentException("Value must between 1 and 255 characters");
            if (!value.All(x => x >= 32 && x <= 126))
                throw new ArgumentException("Value must be ASCII printable");

            return new IdempotencyKey(value);
        }
    }
}