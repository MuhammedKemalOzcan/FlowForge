namespace FlowForge.Domain.ValueObjects
{
    public record Url
    {
        public string Value { get; }

        private Url(string value)
        {
            //Value = uri.ToString();
            Value = value;
        }

        public static Url Create(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Url cannot be empty!");

            //verilen string değeri url formatında mı.
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
                throw new ArgumentException($"{value} is not a valid absolute URL.");

            if (uri.Scheme != "https")
                throw new ArgumentException("URL must use HTTPS scheme.");

            if (IsLocalOrPrivate(uri.Host))
                throw new ArgumentException("URL cannot point to local or private addresses.");

            return new Url(uri.ToString());
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