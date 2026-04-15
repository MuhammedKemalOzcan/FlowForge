using FlowForge.Domain.Errors;
using System.Text.RegularExpressions;

namespace FlowForge.Domain.ValueObjects
{
    public record EventType
    {
        public string Value { get; init; }

        private EventType(string value)
        {
            Value = value;
        }

        public static Result<EventType> Create(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Result<EventType>.Failure(DomainErrors.EventType.Empty);
            if (value.Length > 100)
                return Result<EventType>.Failure(DomainErrors.EventType.TooLong);
            if (!IsValidFormat(value))
                return Result<EventType>.Failure(DomainErrors.EventType.InvalidFormat);

            value = value.Trim().ToLower();

            return Result<EventType>.Success(new EventType(value));
        }

        //namespace.action kontrolü
        private static bool IsValidFormat(string value)
        {
            string pattern = @"^[a-z][a-z0-9_]*(\.[a-z0-9_]+)+$";
            return Regex.IsMatch(value, pattern);
        }
    }
}