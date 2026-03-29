using Buddy.Application.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Infrastructure.Services
{
    public class ApiKeyValidationService : IApiKeyValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiKeyValidationService> _logger;

        public ApiKeyValidationService(HttpClient httpClient, ILogger<ApiKeyValidationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateSimliKeyAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return (false, "Simli API anahtarı boş olamaz.");
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.simli.ai/compose/ice");
            request.Headers.TryAddWithoutValidation("x-simli-api-key", apiKey);

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                _logger.LogWarning("Simli key validation failed with status code {StatusCode}.", response.StatusCode);
                return (false, "Simli API anahtarı geçersiz veya kullanım dışı görünüyor.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Simli key validation request failed.");
                return (false, "Simli API anahtarı doğrulanamadı. Lütfen anahtarınızı kontrol edip tekrar deneyin.");
            }
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateElevenLabsKeyAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return (false, "ElevenLabs API anahtarı boş olamaz.");
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.elevenlabs.io/v1/user");
            request.Headers.TryAddWithoutValidation("xi-api-key", apiKey);

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                _logger.LogWarning("ElevenLabs key validation failed with status code {StatusCode}.", response.StatusCode);
                return (false, "ElevenLabs API anahtarı geçersiz veya kullanım dışı görünüyor.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "ElevenLabs key validation request failed.");
                return (false, "ElevenLabs API anahtarı doğrulanamadı. Lütfen anahtarınızı kontrol edip tekrar deneyin.");
            }
        }
    }
}
