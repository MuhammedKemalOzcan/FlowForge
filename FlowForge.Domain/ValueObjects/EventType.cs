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

        public static EventType Create(string value)
        {
            if (value.Length > 100)
                throw new ArgumentException("Event Type cannot be longer than 100 characters.");
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Event type cannot be null");
            if (!IsValidFormat(value))
                throw new ArgumentException("\"Event type must follow 'namespace.action' format (e.g., 'payment.succeeded').\"");

            value = value.Trim();

            return new EventType(value);
        }

        //namespace.action kontrolü
        private static bool IsValidFormat(string value)
        {
            string pattern = @"^[a-z][a-z0-9_]*(\.[a-z0-9_]+)+$";
            return Regex.IsMatch(value, pattern);
        }
    }
}