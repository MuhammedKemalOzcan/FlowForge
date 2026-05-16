using FlowForge.Domain.ValueObjects;

namespace FlowForge.Application.Abstractions
{
    public interface IApiKeyValidationCache
    {
        Task RemoveAsync(HashedApiKey hashedKey);
    }
}