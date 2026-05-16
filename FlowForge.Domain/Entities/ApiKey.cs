using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using FlowForge.Domain.ValueObjects;
using System.Security.Cryptography;

namespace FlowForge.Domain.Entities
{
    public class ApiKey
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public HashedApiKey Key { get; private set; }
        public string Name { get; private set; }
        public string Prefix { get; private set; }
        public KeyStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? LastUsedAt { get; private set; }

        private ApiKey()
        { }

        public static ApiKeyCreationResult Create(Guid tenantId, string name)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));

            var plainTextKey = Generate();
            var plainKeysPrefix = plainTextKey[..16];
            var hashedKey = HashedApiKey.FromPlainText(plainTextKey);

            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Key = hashedKey,
                Name = name.Trim(),
                Prefix = plainKeysPrefix,
                Status = KeyStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return new ApiKeyCreationResult(plainTextKey, apiKey);
        }

        private static string Generate(int keyBytes = 32)
        {
            byte[] randomBytes = new byte[keyBytes];

            // .NET 6 ve üzeri sürümlerde, RandomNumberGenerator.Fill() yöntemi, belirtilen byte dizisini rastgele verilerle doldurur. Bu yöntem, kriptografik olarak güvenli rastgele sayılar üretmek için kullanılır ve genellikle API anahtarları gibi güvenlik açısından kritik veriler oluşturmak için tercih edilir.
            RandomNumberGenerator.Fill(randomBytes);

            var randomParts = Convert.ToHexString(randomBytes);

            return $"fk_live_{randomParts}";
        }

        public Result Revoke()
        {
            if (Status == KeyStatus.Revoked) return Result.Failure(DomainErrors.ApiKey.AlreadyRevoked);
            Status = KeyStatus.Revoked;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public void RecordUsage()
        {
            if (Status == KeyStatus.Revoked) throw new InvalidOperationException("Cannot record usage for a revoked key.");
            LastUsedAt = DateTime.UtcNow;
        }

        public bool IsUsable()
        {
            if (Status == KeyStatus.Revoked) return false;
            return true;
        }
    }
}