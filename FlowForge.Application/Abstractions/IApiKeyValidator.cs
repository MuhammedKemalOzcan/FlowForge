using FlowForge.Application.Abstractions.Models;
using FlowForge.Domain.Errors;

namespace FlowForge.Application.Abstractions
{
    public interface IApiKeyValidator
    {
        Task<Result<ApiKeyValidationResult>> ValidateAsync(string plainKey, CancellationToken cancellationToken);
    }
}