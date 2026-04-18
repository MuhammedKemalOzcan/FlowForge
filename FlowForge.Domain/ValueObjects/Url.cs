using FlowForge.Domain.Errors;

namespace FlowForge.Domain.ValueObjects
{
    public record Url
    {
        public string Value { get; private set; }

        private Url(string value)
        {
            //Value = uri.ToString();
            Value = value;
        }

        public static Result<Url> Create(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Result<Url>.Failure(DomainErrors.Url.Empty);
            //verilen string değeri url formatında mı.
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
                return Result<Url>.Failure(DomainErrors.Url.InvalidUrl);

            if (uri.Scheme != "https")
                return Result<Url>.Failure(DomainErrors.Url.InvalidScheme);

            if (IsLocalOrPrivate(uri.Host))
                return Result<Url>.Failure(DomainErrors.Url.LocalUrl);

            return Result<Url>.Success(new Url(uri.ToString()));
        }

        private static bool IsLocalOrPrivate(string host)
        {
            if (host.StartsWith("172."))
            {
                var parts = host.Split('.');
                if (parts.Length >= 2 && int.TryParse(parts[1], out var secondOctet))
                {
                    if (secondOctet >= 16 && secondOctet <= 31)
                    {
                        return true;
                    }
                }
            }

            //SSRF protection (Localhost engelleme)
            return host is "localhost" or "127.0.0.1" or "0.0.0.0"
            || host.StartsWith("192.168.")
            || host.StartsWith("10.")
            || host.StartsWith("127.")
            || host.StartsWith("169.254");
        }
    }
}