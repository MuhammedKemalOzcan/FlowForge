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
            var eventType = EventType.Create(inputs);

            //Assert
            eventType.Should().NotBeNull();
            eventType.Value.Should().Be(inputs);
        }

        [Theory]
        [InlineData("paymentsucceeded")]
        [InlineData("order")]
        public void Create_WithoutDotSeperator_ThrowsArgumentException(string inputs)
        {
            //Act
            Action act = () => EventType.Create(inputs);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*format*");
        }

        [Theory]
        [InlineData("Payment.succeeded")]
        [InlineData("PAYMENT.SUCCEEDED")]
        [InlineData("payment.Succeeded")]
        public void Create_WithUppercaseLetters_ThrowsArgumentException(string inputs)
        {
            //Act
            Action act = () => EventType.Create(inputs);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*format*");
        }

        [Theory]
        [InlineData("Payment-succeeded")]
        [InlineData("payment/succeded")]
        [InlineData("payment@succeeded")]
        public void Create_WithInvalidCharacters_ThrowsArgumentException(string inputs)
        {
            //Act
            Action act = () => EventType.Create(inputs);

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*format*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void Create_WithnullOrWihtespace_ThrowsArgumentException(string? inputs)
        {
            //Act
            Action act = () => EventType.Create(inputs!);

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Create_WithValueLongerThan100Chars_ThrowsArgumentException()
        {
            //Arrange
            var input = "a." + new string('b', 99);

            //Act
            Action act = () => EventType.Create(input);

            //Assert
            act.Should().Throw<ArgumentException>();
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
            eventType.Value.Should().Be(input);
        }
    }
}