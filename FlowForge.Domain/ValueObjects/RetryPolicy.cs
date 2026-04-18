using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;

namespace FlowForge.Domain.ValueObjects
{
    public record RetryPolicy
    {
        public int MaxAttempts { get; private set; }
        public BackoffStrategy Strategy { get; private set; }
        public TimeSpan InitialDelay { get; private set; }
        public TimeSpan MaxDelay { get; private set; }
        public TimeSpan TimeOut { get; private set; }

        private RetryPolicy(int maxAttempt, BackoffStrategy strategy, TimeSpan initialDelay, TimeSpan maxDelay, TimeSpan timeout)
        {
            MaxAttempts = maxAttempt;
            Strategy = strategy;
            InitialDelay = initialDelay;
            MaxDelay = maxDelay;
            //HTTP isteğinin ne kadar sürede yanıt vermesi gerektiği.
            TimeOut = timeout;
        }

        public static Result<RetryPolicy> Create(int maxAttempt, BackoffStrategy strategy, TimeSpan initialDelay, TimeSpan maxDelay, TimeSpan timeout)
        {
            if (maxAttempt < 1 || maxAttempt > 10)
                return Result<RetryPolicy>.Failure(DomainErrors.RetryPolicy.InvalidMaxAttemptRange);
            if (initialDelay < TimeSpan.Zero)
                return Result<RetryPolicy>.Failure(DomainErrors.RetryPolicy.NegativeInitialDelay);
            if (maxDelay < initialDelay)
                return Result<RetryPolicy>.Failure(DomainErrors.RetryPolicy.InvalidDelayRange);
            if (timeout <= TimeSpan.Zero)
                return Result<RetryPolicy>.Failure(DomainErrors.RetryPolicy.NegativeTimeout);

            return Result<RetryPolicy>.Success(new RetryPolicy(maxAttempt, strategy, initialDelay, maxDelay, timeout));
        }

        public static RetryPolicy Default() => new(
            maxAttempt: 5,
            strategy: BackoffStrategy.Exponential,
            initialDelay: TimeSpan.FromSeconds(1),
            maxDelay: TimeSpan.FromMinutes(5),
            timeout: TimeSpan.FromSeconds(10)
            );

        public TimeSpan CalculateDelayFor(int attemptNumber)
        {
            if (attemptNumber < 1 || attemptNumber > MaxAttempts)
                throw new ArgumentOutOfRangeException(nameof(attemptNumber), $"Attempt number must be between 1 and {MaxAttempts}.");

            var delay = Strategy switch
            {
                BackoffStrategy.Fixed => InitialDelay,
                BackoffStrategy.Linear => InitialDelay * attemptNumber,
                BackoffStrategy.Exponential => InitialDelay * Math.Pow(2, attemptNumber - 1),
                _ => InitialDelay
            };

            return delay > MaxDelay ? MaxDelay : delay;
        }

        public bool IsLastAttempt(int attemptNumber) =>
            attemptNumber >= MaxAttempts;
    }
}