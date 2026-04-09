using FlowForge.Domain.Enums;

namespace FlowForge.Domain.ValueObjects
{
    public record RetryPolicy
    {
        public int MaxAttempts { get; init; }
        public BackoffStrategy Strategy { get; init; }
        public TimeSpan InitialDelay { get; init; }
        public TimeSpan MaxDelay { get; init; }
        public TimeSpan TimeOut { get; init; }

        private RetryPolicy(int maxAttempt, BackoffStrategy strategy, TimeSpan initialDelay, TimeSpan maxDelay, TimeSpan timeout)
        {
            MaxAttempts = maxAttempt;
            Strategy = strategy;
            InitialDelay = initialDelay;
            MaxDelay = maxDelay;
            //HTTP isteğinin ne kadar sürede yanıt vermesi gerektiği.
            TimeOut = timeout;
        }

        public static RetryPolicy Create(int maxAttempt, BackoffStrategy strategy, TimeSpan initialDelay, TimeSpan maxDelay, TimeSpan timeout)
        {
            if (maxAttempt < 1 || maxAttempt > 10)
                throw new ArgumentException("Max attempts must be between 1 and 10");
            if (initialDelay < TimeSpan.Zero)
                throw new ArgumentException("Initial delay cannot be nagative");
            if (maxDelay < initialDelay)
                throw new ArgumentException("MaxDelay must be >= InitialDelay.");
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentException("timeout must be positive");

            return new RetryPolicy(maxAttempt, strategy, initialDelay, maxDelay, timeout);
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
                throw new ArgumentOutOfRangeException(nameof(attemptNumber));

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