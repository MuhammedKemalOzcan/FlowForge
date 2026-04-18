using System.Security.Cryptography;
using System.Text;

namespace FlowForge.Domain.ValueObjects
{
    public record SigningSecret
    {
        internal string Value { get; private set; }

        private SigningSecret(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Secret Key cannot be empty.");
            if (value.Length < 40)
                throw new ArgumentException("Secret Key is too short. Security Risk!");
            Value = value;
        }

        //"Bu nesne her ne amaçla yazdırılmak istenirse istensin (log, debugger, UI), sakın içeriğini gösterme, sadece 'PROTECTED' yaz."
        public override string ToString() => "***PROTECTED***";

        public static SigningSecret Create()
        {
            var newValue = GenerateSecret();
            return new SigningSecret(newValue);
        }

        private static string GenerateSecret(int keyBytes = 32)
        {
            //Ram'de 32 bytle'lık boş bir array tanımlanır.
            byte[] randomBytes = new byte[keyBytes];

            RandomNumberGenerator.Fill(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        public string ComputeSignature(string payload)
        {
            byte[] keyBytes = Convert.FromBase64String(Value);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(payloadBytes);

                string hmacHex = Convert.ToHexString(hashBytes).ToLowerInvariant();

                return hmacHex;
            }
        }
    }
}