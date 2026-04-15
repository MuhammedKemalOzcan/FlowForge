using FlowForge.Domain.Enums;

namespace FlowForge.Domain.Errors
{
    public sealed record Error
    {
        public string Message { get; private set; }
        public string Code { get; private set; }
        public ErrorType ErrorType { get; private set; }

        private Error(string code, string message, ErrorType type)
        {
            Code = code;
            Message = message;
            ErrorType = type;
        }

        public static readonly Error None = new Error(string.Empty, string.Empty, ErrorType.None);

        public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
        public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    }
}