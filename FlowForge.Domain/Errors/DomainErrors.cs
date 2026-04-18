namespace FlowForge.Domain.Errors
{
    public static class DomainErrors
    {
        public static class EventType
        {
            public static Error Empty => Error.Validation("EventType.Empty", "Event type cannot be empty.");
            public static Error TooLong => Error.Validation("EventType.TooLong", "Event Type cannot be longer than 100 characters.");
            public static Error InvalidFormat => Error.Validation("EventType.InvalidFormat", "\"Event type must follow 'namespace.action' format (e.g., 'payment.succeeded').\"");
        }

        public static class IdempotencyKey
        {
            public static Error Empty => Error.Validation("IdempotencyKey.Empty", "Idempotency key cannot be empty.");
            public static Error InvalidLength => Error.Validation("IdempotencyKey.InvalidLength", "Value must be between 1 and 255 characters.");
            public static Error InvalidFormat => Error.Validation("IdempotencyKey.InvalidFormat", "Value must be ASCII printable");
        }

        public static class Url
        {
            public static Error Empty => Error.Validation("Url.Empty", "Url cannot be empty.");
            public static Error InvalidUrl => Error.Validation("Url.InvalidUrl", "This format is not a valid absolute URL.");
            public static Error InvalidScheme => Error.Validation("Url.InvalidScheme", "URL must use HTTPS scheme.");
            public static Error LocalUrl => Error.Validation("Url.LocalUrl", "URL cannot point to local or private addresses.");
        }

        public static class RetryPolicy
        {
            public static Error InvalidMaxAttemptRange => Error.Validation("RetryPolicy.InvalidMaxAttemptRange", "Max attempts must be between 1 and 10.");
            public static Error NegativeInitialDelay => Error.Validation("RetryPolicy.NegativeInitialDelay", "Initial delay cannot be negative");
            public static Error NegativeTimeout => Error.Validation("RetryPolicy.NegativeTimeout", "Timeout cannot be negative");
            public static Error InvalidDelayRange => Error.Validation("RetryPolicy.InvalidDelayRange", "MaxDelay must be >= InitialDelay.");
            public static Error AttemptOutOfRange => Error.Validation("RetryPolicy.AttemptOutOfRange", "Attempt number must be between 1 and 10");
        }

        public static class EndpointName
        {
            public static Error NameEmpty => Error.Validation("EndpointName.NameEmpty", "Name cannot be empty.");
            public static Error InvalidNameLength => Error.Validation("EndpointName.InvalidNameLength", "Name must be between 2 and 100 characters.");
        }

        public static class WebhookEndpoint
        {
            public static Error AlreadyDeactivated => Error.Conflict("WebhookEndpoint.AlreadyDeactivated", "The webhook endpoint is already deactivated.");
        }

        public static class ExternalIdentityId
        {
            public static Error Empty => Error.Validation("ExternalIdentityId.Empty", "External identity ID cannot be empty.");
        }

        public static class ApiKey
        {
            public static Error AlreadyRevoked => Error.Validation("ApiKey.AlreadyRevoked", "This key is already revoked cannot be revoked again.");
        }

        public static class Email
        {
            public static Error InvalidEmailFormat => Error.Validation("Email.InvalidEmailFormat", "Invalid Email format!");
            public static Error Empty => Error.Validation("Email.Empty", "Email cannot be empty.");
        }

        public static class Tenant
        {
            public static Error ReachedMaxMember => Error.LimitExceeded("Tenant.ReachedMaxMember", "Max number of members has been reached.");
            public static Error MemberAlreadyExist => Error.Conflict("Tenant.MemberAlreadyExist", "This user is already a member.");
            public static Error MemberCannotFound => Error.NotFound("Tenant.MemberCannotFound", "This user is not a member.");
            public static Error OwnerCannotBeRemoved => Error.Forbidden("Tenant.OwnerCannotBeRemoved", "Owner cannot be removed.");
        }
    }
}