using FlowForge.Application.Abstractions;
using FlowForge.Application.Abstractions.Models;
using FlowForge.Application.Data;
using FlowForge.Domain.Errors;
using FlowForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace FlowForge.Infrastructure.Authentication
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IFlowForgeApiDbContext _context;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        public ApiKeyValidator(IConnectionMultiplexer redis, IFlowForgeApiDbContext context)
        {
            _redis = redis;
            _context = context;
        }

        public async Task<Result<ApiKeyValidationResult>> ValidateAsync(string plainKey, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(plainKey))
                return Result<ApiKeyValidationResult>.Failure(DomainErrors.ApiKey.NotFound);

            var hashedKey = HashedApiKey.FromPlainText(plainKey);
            var cacheKey = BuildCacheKey(hashedKey);

            //Cache'lenmiş mi diye kontrol et eğer cachelnmişse db'ye gitme
            var cached = await TryGetFromCacheAsync(cacheKey);
            if (cached is not null) return Result<ApiKeyValidationResult>.Success(cached);

            var apiKey = await _context.ApiKeys
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.Key.Value == hashedKey.Value, cancellationToken);

            if (apiKey is null)
                return Result<ApiKeyValidationResult>.Failure(DomainErrors.ApiKey.NotFound);

            if (!apiKey.IsUsable())
                return Result<ApiKeyValidationResult>.Failure(DomainErrors.ApiKey.Revoked);

            var result = new ApiKeyValidationResult(apiKey.TenantId, apiKey.Id);
            await TrySetToCache(cacheKey, result);

            return Result<ApiKeyValidationResult>.Success(result);
        }

        private static string BuildCacheKey(HashedApiKey hashedKey) => $"flowforge:apikey:{hashedKey.Value}";

        private async Task<ApiKeyValidationResult?> TryGetFromCacheAsync(string cacheKey)
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(cacheKey);

            if (!value.HasValue)
                return null;

            //Redisin döndürdüğü JSON objesinie tekrardan ApiKeyValidationResult nesnesine dönüştürür.
            return JsonSerializer.Deserialize<ApiKeyValidationResult>(value!);
        }

        private async Task TrySetToCache(string cacheKey, ApiKeyValidationResult result)
        {
            var db = _redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(result);
            await db.StringSetAsync(cacheKey, serialized, CacheTtl);
        }
    }
}