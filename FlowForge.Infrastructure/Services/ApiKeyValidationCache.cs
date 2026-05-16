using FlowForge.Application.Abstractions;
using FlowForge.Domain.ValueObjects;
using StackExchange.Redis;

namespace FlowForge.Infrastructure.Services
{
    public class ApiKeyValidationCache : IApiKeyValidationCache
    {
        private readonly IConnectionMultiplexer _redis;

        public ApiKeyValidationCache(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task RemoveAsync(HashedApiKey hashedKey)
        {
            var db = _redis.GetDatabase();

            var cacheKey = $"flowforge:apikey:{hashedKey.Value}";
            await db.KeyDeleteAsync(cacheKey);
        }
    }
}