using FlowForge.Domain.Services;
using StackExchange.Redis;

namespace FlowForge.Infrastructure.Services
{
    public class RedisRateLimiter : IRateLimiter
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisRateLimiter(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<bool> IsAllowedAsync(Guid tenantId, int MaxRequestsPerMinute)
        {
            var db = _redis.GetDatabase();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var windowStart = now - 60;
            var key = $"rate:tenant:{tenantId}";

            //60 saniyeden eski kayıtları sil.
            await db.SortedSetRemoveRangeByScoreAsync(key, 0, windowStart);

            //Son 60 saniyedeki istek sayısı.
            var currentCount = await db.SortedSetLengthAsync(key);

            if (currentCount >= MaxRequestsPerMinute) return false;

            await db.SortedSetAddAsync(key, Guid.NewGuid().ToString(), now);

            await db.KeyExpireAsync(key, TimeSpan.FromMinutes(2));

            return true;
        }
    }
}