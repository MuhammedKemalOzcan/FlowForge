using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using FlowForge.Domain.ValueObjects;
using FluentAssertions;

namespace FlowForge.Domain.Tests.ValueObjects
{
    public class EventTypeTests
    {
        [Theory]
        [InlineData("payment.succeeded")]
        [InlineData("order.created")]
        [InlineData("customer.subscription.deleted")]
        [InlineData("payment.subscription_renewed")]
        [InlineData("user.login2")]
        [InlineData("a.b")]
        public void Create_WithValidFormat_ReturnsInstance(string inputs)
        {
            //Act
            var result = EventType.Create(inputs);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Value.Should().Be(inputs);
        }

        [Theory]
        [InlineData("paymentsucceeded")]
        [InlineData("order")]
        public void Create_WithoutDotSeperator_ReturnsFailureResult(string inputs)
        {
            //Act
            var result = EventType.Create(inputs!);

            //Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DomainErrors.EventType.InvalidFormat);
        }

        [Theory]
        [InlineData("Payment.succeeded")]
        [InlineData("PAYMENT.SUCCEEDED")]
        [InlineData("payment.Succeeded")]
        public void Create_WithUppercaseLetters_ReturnsFailureResult(string inputs)
        {
            //Act
            var result = EventType.Create(inputs!);

            //Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DomainErrors.EventType.InvalidFormat);
        }

        [Theory]
        [InlineData("Payment-succeeded")]
        [InlineData("payment/succeded")]
        [InlineData("payment@succeeded")]
        public void Create_WithInvalidCharacters_ReturnsFailureResult(string inputs)
        {
            //Act
            var result = EventType.Create(inputs!);

            //Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DomainErrors.EventType.InvalidFormat);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void Create_WithNullOrWhitespace_ReturnsFailureResult(string? inputs)
        {
            //Act
            var result = EventType.Create(inputs!);

            //Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error.Code.Should().Be("EventType.Invalid");
            result.Error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public void Create_WithValueLongerThan100Chars_ReturnsFailureResult()
        {
            //Arrange
            var input = "a." + new string('b', 99);

            //Act
            var result = EventType.Create(input);

            //Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DomainErrors.EventType.TooLong);
        }

        [Fact]
        public void Create_WithValueAtMaxLength_CreatesInstance()
        {
            //Arrange
            var input = new string('a', 49) + "." + new string('b', 50);

            //Act
            var eventType = EventType.Create(input);

            //Arrange
            eventType.Should().NotBeNull();
            eventType.Data.Value.Should().Be(input);
        }
    }
}