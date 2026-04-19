using FlowForge.Domain.ValueObjects;
using FluentAssertions;

namespace FlowForge.Domain.Tests.ValueObjects
{
    public class IdempotencyKeyTests
    {
        [Fact]
        public void Create_WithValidKey_ReturnsIdempotencyKeyInstance()
        {
            //Arrange
            var validKey = "evt_abc123";

            //Act
            var idempotencyKey = IdempotencyKey.Create(validKey);

            //Assert
            idempotencyKey.Should().NotBeNull();
            idempotencyKey.Data.Value.Should().Be(validKey);
        }

        [Theory]
        [InlineData("evt_abc123")]
        [InlineData("payment-20260408-xyz")]
        [InlineData("550e8400-e29b-41d4-a716-446655440000")]
        [InlineData("a")]
        [InlineData("UPPER_CASE")]
        public void Create_WithVariousValidFormats_ReturnsInstance(string validKeys)
        {
            //Act
            Action act = () => IdempotencyKey.Create(validKeys);

            //Assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Create_WithNullOrWhitespaceKey_ReturnsFailureResult(string validKeys)
        {
            //Act
            Action act = () => IdempotencyKey.Create(validKeys);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*empty*");
        }

        [Fact]
        public void Create_WithKeyLongerThan255Chars_ReturnsFailureResult()
        {
            //Arrange
            var input = new string('a', 256);

            //Act
            Action act = () => IdempotencyKey.Create(input);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*between*");
        }

        [Fact]
        public void Create_WithKeyAtMaxLength_CreatesInstance()
        {
            //Arrange
            var input = new string('a', 255);

            //Act
            Action act = () => IdempotencyKey.Create(input);

            //Assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData("hello😀world")]
        [InlineData("türkçe_key")]
        [InlineData("key\u0001invisible")]
        [InlineData("key\u007Fdel")]
        public void Create_WithNonAsciiCharacters_ReturnsFailureResult(string invalidInputs)
        {
            //Act
            Action act = () => IdempotencyKey.Create(invalidInputs);

            //ASsert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("abc123")]
        [InlineData("key-with-dashes")]
        [InlineData("key_with_underscores")]
        [InlineData("key.with.dots")]
        [InlineData("key!@#$%^&*()")]
        public void Create_WithAsciiPrintableCharacters_CreatesInstance(string validInput)
        {
            //Act
            Action act = () => IdempotencyKey.Create(validInput);

            //Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Equality_TwoKeysWithSameValue_AreEqual()
        {
            //Arrange
            var input1 = "test-input";
            var input2 = "test-input";

            //Act
            var key1 = IdempotencyKey.Create(input1);
            var key2 = IdempotencyKey.Create(input2);

            //Arrange
            key1.Should().Be(key2);
        }
    }
}