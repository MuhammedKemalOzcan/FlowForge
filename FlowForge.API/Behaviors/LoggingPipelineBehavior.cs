using FlowForge.Domain.Errors;
using MediatR;
using System.Diagnostics;

namespace FlowForge.API.Behaviors
{
    public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

        public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting request {RequestName}, {DateTimeUtc}",
                typeof(TRequest).Name,
                DateTime.UtcNow);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = await next();

                stopwatch.Stop();

                if (!result.IsSuccess)
                {
                    _logger.LogError("Request {RequestName}, {Error} failed at, {DateTimeUtc}, after {ElapsedMilliseconds} ms",
                typeof(TRequest).Name,
                result.Error,
                DateTime.UtcNow,
                stopwatch.ElapsedMilliseconds);
                }

                _logger.LogInformation("Completed request {RequestName}, {DateTimeUtc},in {ElapsedMilliseconds} ms",
                typeof(TRequest).Name,
                DateTime.UtcNow,
                stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                ex,
                "Request {RequestName} threw an exception after {ElapsedMilliseconds} ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}