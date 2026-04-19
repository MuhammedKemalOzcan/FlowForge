using FlowForge.Domain.Errors;

namespace FlowForge.Domain.ValueObjects
{
    public record ExternalIdentityId
    {
        public string ExternalId { get; private set; }

        private ExternalIdentityId()
        {
        }
        private ExternalIdentityId(string id)
        {
            ExternalId = id;
        }

        public static Result<ExternalIdentityId> Create(string id)
        {
            if (string.IsNullOrEmpty(id)) return Result<ExternalIdentityId>.Failure(DomainErrors.ExternalIdentityId.Empty);

            return Result<ExternalIdentityId>.Success(new ExternalIdentityId(id));
        }
    }
}