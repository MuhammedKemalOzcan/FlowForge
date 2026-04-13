using FlowForge.Domain.ValueObjects;
using FluentAssertions;

namespace FlowForge.Domain.Tests.ValueObjects
{
    public class SigningSecretTests
    {
        [Fact]
        public void CreateNew_ReturnsValidSigningSecretInstance()
        {
            // Act
            var secret = SigningSecret.Create();

            // Assert
            secret.Should().NotBeNull();
            secret.Value.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void CreateNew_CalledMultipleTimes_ProducesUniqueSecrets()
        {
            // Arrange & Act
            var secrets = new HashSet<string>();
            for (int i = 0; i < 100; i++)
            {
                var secret = SigningSecret.Create();
                secrets.Add(secret.Value);
            }

            // Assert
            secrets.Should().HaveCount(100);
        }

        [Fact]
        public void CreateNew_GeneratesBase64EncodedValue()
        {
            // Arrange & Act
            var secret = SigningSecret.Create();

            // Assert
            Action decode = () => Convert.FromBase64String(secret.Value);
            decode.Should().NotThrow();
        }

        [Fact]
        public void CreateNew_GeneratesSecretWithAtLeast32BytesOfEntropy()
        {
            // Arrange & Act
            var secret = SigningSecret.Create();
            var decodedBytes = Convert.FromBase64String(secret.Value);

            // Assert
            decodedBytes.Length.Should().BeGreaterThanOrEqualTo(32);
        }

        [Fact]
        public void ComputeSignature_CalledTwiceWithSameSecretAndPayload_ReturnsSameSignature()
        {
            // Arrange
            var secret = CreateTestSecret();
            var payload = "{\"event\":\"payment.succeeded\",\"amount\":1000}";

            // Act
            var signature1 = secret.ComputeSignature(payload);
            var signature2 = secret.ComputeSignature(payload);

            // Assert
            signature1.Should().Be(signature2);
        }

        [Fact]
        public void ComputeSignature_WithDifferentPayloads_ReturnsDifferentSignatures()
        {
            // Arrange
            var secret = CreateTestSecret();
            var payload1 = "{\"amount\":1000}";
            var payload2 = "{\"amount\":2000}";

            // Act
            var signature1 = secret.ComputeSignature(payload1);
            var signature2 = secret.ComputeSignature(payload2);

            // Assert
            signature1.Should().NotBe(signature2);
        }

        [Fact]
        public void ComputeSignature_WithPayloadDifferingByOneCharacter_ReturnsCompletelyDifferentSignatures()
        {
            // Arrange
            var secret = CreateTestSecret();
            var payload1 = "hello world";
            var payload2 = "hello World";  // Sadece W büyük harf

            // Act
            var signature1 = secret.ComputeSignature(payload1);
            var signature2 = secret.ComputeSignature(payload2);

            // Assert
            signature1.Should().NotBe(signature2);
        }

        [Fact]
        public void ComputeSignature_WithDifferentSecrets_ReturnsDifferentSignatures()
        {
            // Arrange
            var secret1 = SigningSecret.Create();
            var secret2 = SigningSecret.Create();
            var payload = "{\"amount\":1000}";

            // Act
            var signature1 = secret1.ComputeSignature(payload);
            var signature2 = secret2.ComputeSignature(payload);

            // Assert
            signature1.Should().NotBe(signature2);
        }

        [Fact]
        public void ComputeSignature_ReturnsHexStringOf64Characters()
        {
            // Arrange
            var secret = CreateTestSecret();
            var payload = "test payload";

            // Act
            var signature = secret.ComputeSignature(payload);

            // Assert
            signature.Should().HaveLength(64);
        }

        [Fact]
        public void ComputeSignature_ReturnsLowercaseHexString()
        {
            // Arrange
            var secret = CreateTestSecret();
            var payload = "test payload";

            // Act
            var signature = secret.ComputeSignature(payload);

            // Assert
            signature.Should().MatchRegex("^[0-9a-f]{64}$");
        }

        [Fact]
        public void ComputeSignature_WithEmptyPayload_ReturnsValidSignature()
        {
            // Arrange
            var secret = CreateTestSecret();

            // Act
            var signature = secret.ComputeSignature("");

            // Assert
            signature.Should().NotBeNullOrWhiteSpace();
            signature.Should().HaveLength(64);
        }

        //Helper
        private static SigningSecret CreateTestSecret() => SigningSecret.Create();
    }
}