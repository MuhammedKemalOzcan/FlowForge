using System.Security.Cryptography;
using System.Text;

namespace FlowForge.Domain.ValueObjects
{
    public record HashedApiKey
    {
        public string Value { get; private set; }

        private HashedApiKey() { }
        private HashedApiKey(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Hashed API Key cannot be empty.");
            Value = value;
        }

        //Kullanıcının gönderdiği ham key'i alıp hash'leyip veritabanındaki hashlenmiş key ile karşılaştırmak için kullanılır.
        public static HashedApiKey FromPlainText(string plainTextKey)
        {
            if (string.IsNullOrEmpty(plainTextKey)) throw new ArgumentException("Plain text API Key cannot be empty.");

            var keyBytes = Encoding.UTF8.GetBytes(plainTextKey);

            var hashBytes = SHA256.HashData(keyBytes);
            var hashString = Convert.ToHexString(hashBytes).ToLower();

            return new HashedApiKey(hashString);
        }

        public static HashedApiKey FromHash(string existingHash)
        {
            if (string.IsNullOrEmpty(existingHash)) throw new ArgumentException("Existing hash cannot be empty.");
            return new HashedApiKey(existingHash);
        }
    }
}