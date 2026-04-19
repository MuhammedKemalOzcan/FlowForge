using FlowForge.Domain.Errors;

namespace FlowForge.Domain.ValueObjects
{
    public record Email
    {
        public string Value { get; private set; }

        private Email() { }
        private Email(string value)
        {
            Value = value;
        }

        public static Result<Email> Create(string email)
        {
            email = email.Trim();
            if (string.IsNullOrWhiteSpace(email)) return Result<Email>.Failure(DomainErrors.Email.Empty);
            if (!System.Net.Mail.MailAddress.TryCreate(email, out _)) return Result<Email>.Failure(DomainErrors.Email.InvalidEmailFormat);

            return Result<Email>.Success(new Email(email));
        }
    }
}