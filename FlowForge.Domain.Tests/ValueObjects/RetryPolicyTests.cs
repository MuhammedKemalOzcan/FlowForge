using FlowForge.Domain.Enums;
using FlowForge.Domain.ValueObjects;
using FluentAssertions;

namespace FlowForge.Domain.Tests.ValueObjects
{
    public class RetryPolicyTests
    {
        //Constructor Validation Tests:
        [Fact]
        public void Constructor_WithValidParameters_CreatesRetryPolicy()
        {
            //Arrange & Act: Oluşturmak zaten act aşamasına da dahil.
            var retryPolicy = RetryPolicy.Create(
                5,
                Enums.BackoffStrategy.Exponential,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromSeconds(10)
                );

            //Assert
            retryPolicy.Data.MaxAttempts.Should().Be(5);
            retryPolicy.Data.Strategy.Should().Be(Enums.BackoffStrategy.Exponential);
            retryPolicy.Data.InitialDelay.Should().Be(TimeSpan.FromSeconds(1));
            retryPolicy.Data.MaxDelay.Should().Be(TimeSpan.FromMinutes(5));
            retryPolicy.Data.TimeOut.Should().Be(TimeSpan.FromSeconds(10));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(11)]
        [InlineData(150)]
        public void Constructor_WithInvalidMaxAttempt_ReturnsFailureResult(int invalidMaxAttempts)
        {
            //Act
            Action act = () => RetryPolicy.Create(
                invalidMaxAttempts,
                Enums.BackoffStrategy.Exponential,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromSeconds(10)
                );

            //Assert
            act.Should().Throw<ArgumentException>().WithMessage("*max attempts*");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(5)]
        public void Constructor_WithValidMaxAttempt_CreatesRetryPolicy(int validMaxAttempts)
        {
            //Act
            Action act = () => RetryPolicy.Create(
                validMaxAttempts,
                Enums.BackoffStrategy.Exponential,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromSeconds(10)
                );

            //Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Constructor_WithNegativeInitialDelay_ReturnsFailureResult()
        {
            //Arrange & Act
            Action act = () => RetryPolicy.Create(
                5,
                Enums.BackoffStrategy.Exponential,
                TimeSpan.FromSeconds(-1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromSeconds(10)
                );

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_WithMaxDelayLessThanInitialDelay_ReturnsFailureResult()
        {
            //Arrange & Act
            Action act = () => RetryPolicy.Create(
                5,
                BackoffStrategy.Exponential,
                TimeSpan.FromMinutes(2),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(10)
                );

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_WithNegativeTimeout_ReturnsFailureResult()
        {
            //Arrange & Act
            Action act = () => RetryPolicy.Create(
                5,
                Enums.BackoffStrategy.Exponential,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromSeconds(-10)
                );

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        //DefaultFactory Test:
        [Fact]
        public void Default_ReturnsExpectedConfiguration()
        {
            //Arrange & Act
            var policy = RetryPolicy.Default();

            //Assert
            policy.MaxAttempts.Should().Be(5);
            policy.Strategy.Should().Be(BackoffStrategy.Exponential);
            policy.InitialDelay.Should().Be(TimeSpan.FromSeconds(1));
            policy.MaxDelay.Should().Be(TimeSpan.FromMinutes(5));
            policy.TimeOut.Should().Be(TimeSpan.FromSeconds(10));
        }

        //CalculateDelayFor() Tests:

        [Theory]
        [InlineData(1, 1)] //1s * 2^0 = 1sn
        [InlineData(2, 2)] //1s * 2^1 = 2 => (attemptNumber,expectedSeconds)
        [InlineData(3, 4)]
        [InlineData(4, 8)]
        [InlineData(5, 16)]
        public void CalculateDelayFor_ExponentialStrategy_ReturnsCorrectDelay(int attemptNumber, int expectedSeconds)
        {
            //Arrange
            var policy = RetryPolicy.Create(
                maxAttempt: 10,
                strategy: BackoffStrategy.Exponential,
                initialDelay: TimeSpan.FromSeconds(1),
                maxDelay: TimeSpan.FromMinutes(10),
                timeout: TimeSpan.FromSeconds(30)
            );

            var delay = policy.Data.CalculateDelayFor(attemptNumber);

            //Assert
            delay.Should().Be(TimeSpan.FromSeconds(expectedSeconds));
        }

        [Fact]
        public void CalculateDelayFor_WhenCalculatedDelayExceedsMaxDelay_ReturnsMaxDelay()
        {
            // Arrange
            var policy = RetryPolicy.Create(
                maxAttempt: 10,
                strategy: BackoffStrategy.Exponential,
                initialDelay: TimeSpan.FromSeconds(1),
                maxDelay: TimeSpan.FromSeconds(5),  // DÜŞÜK cap
                timeout: TimeSpan.FromSeconds(30)
            );

            // Act - 10. attempt için 1 * 2^9 = 512 saniye hesaplanacak
            // Ama MaxDelay=5s olduğu için 5 saniye dönmeli
            var delay = policy.Data.CalculateDelayFor(10);

            // Assert
            delay.Should().Be(TimeSpan.FromSeconds(5));
        }

        [Theory]
        [InlineData(1, 2)]   // 2s * 1 = 2s
        [InlineData(2, 4)]   // 2s * 2 = 4s
        [InlineData(3, 6)]   // 2s * 3 = 6s
        [InlineData(5, 10)]  // 2s * 5 = 10s
        public void CalculateDelayFor_LinearStrategy_ReturnsCorrectDelay(
        int attemptNumber,
        int expectedSeconds)
        {
            // Arrange
            var policy = RetryPolicy.Create(
                maxAttempt: 10,
                strategy: BackoffStrategy.Linear,
                initialDelay: TimeSpan.FromSeconds(2),
                maxDelay: TimeSpan.FromMinutes(10),
                timeout: TimeSpan.FromSeconds(30)
            );

            // Act
            var delay = policy.Data.CalculateDelayFor(attemptNumber);

            // Assert
            delay.Should().Be(TimeSpan.FromSeconds(expectedSeconds));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void CalculateDelayFor_FixedStrategy_AlwaysReturnsInitialDelay(int attemptNumber)
        {
            // Arrange
            var policy = RetryPolicy.Create(
                maxAttempt: 10,
                strategy: BackoffStrategy.Fixed,
                initialDelay: TimeSpan.FromSeconds(3),
                maxDelay: TimeSpan.FromMinutes(10),
                timeout: TimeSpan.FromSeconds(30)
            );

            // Act
            var delay = policy.Data.CalculateDelayFor(attemptNumber);

            // Assert
            delay.Should().Be(TimeSpan.FromSeconds(3));
        }

        [Theory]
        [InlineData(0)]    // sıfır - yasak
        [InlineData(-1)]   // negatif
        [InlineData(6)]    // MaxAttempts (5) + 1
        [InlineData(100)]  // çok büyük
        public void CalculateDelayFor_WithInvalidAttemptNumber_ThrowsArgumentOutOfRangeException(
          int invalidAttempt)
        {
            // Arrange
            var policy = RetryPolicy.Create(
                maxAttempt: 5,
                strategy: BackoffStrategy.Exponential,
                initialDelay: TimeSpan.FromSeconds(1),
                maxDelay: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(10)
            );

            // Act
            Action act = () => policy.Data.CalculateDelayFor(invalidAttempt);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        //IsLastAttempt Tests:
        [Fact]
        public void IsLastAttempt_WhenAttemptNumberEqualsMaxAttempts_ReturnsTrue()
        {
            // Arrange
            var policy = RetryPolicy.Create(
                maxAttempt: 5,
                strategy: BackoffStrategy.Exponential,
                initialDelay: TimeSpan.FromSeconds(1),
                maxDelay: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(10)
            );

            //Assert
            policy.Data.IsLastAttempt(5).Should().Be(true);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void IsLastAttempt_WhenAttemptNumberLessThanMaxAttempts_ReturnsFalse(int attemptNumber)
        {
            var policy = RetryPolicy.Create(
                maxAttempt: 5,
                strategy: BackoffStrategy.Exponential,
                initialDelay: TimeSpan.FromSeconds(1),
                maxDelay: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(10)
            );

            //Assert
            policy.Data.IsLastAttempt(attemptNumber).Should().Be(false);
        }

        [Fact]
        public void IsLastAttempt_WhenAttemptNumberGreaterThanMaxAttempts_ReturnsTrue()
        {
            // Arrange
            var policy = RetryPolicy.Create(
                maxAttempt: 5,
                strategy: BackoffStrategy.Exponential,
                initialDelay: TimeSpan.FromSeconds(1),
                maxDelay: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(10)
            );

            //Assert
            policy.Data.IsLastAttempt(6).Should().Be(true);
        }
    }
}