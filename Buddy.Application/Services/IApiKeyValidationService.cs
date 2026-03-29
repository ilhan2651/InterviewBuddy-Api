using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Services
{
    public interface IApiKeyValidationService
    {
        Task<(bool IsValid, string? ErrorMessage)> ValidateSimliKeyAsync(string apiKey, CancellationToken cancellationToken = default);
        Task<(bool IsValid, string? ErrorMessage)> ValidateElevenLabsKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    }
}
