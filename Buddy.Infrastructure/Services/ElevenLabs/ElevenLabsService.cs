using Buddy.Application.Common.Interfaces;
using Buddy.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Infrastructure.Services.ElevenLabs
{
    public class ElevenLabsService : ITextToSpeechService
    {
        private readonly HttpClient _httpClient;
        private readonly string _systemApiKey;
        private readonly string _voiceId;
        private readonly Microsoft.Extensions.Logging.ILogger<ElevenLabsService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;

        public ElevenLabsService(HttpClient httpClient, IConfiguration configuration, Microsoft.Extensions.Logging.ILogger<ElevenLabsService> logger,
            ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IEncryptionService encryptionService)
        {
            _httpClient = httpClient;
            _systemApiKey = configuration["ElevenLabs:ApiKey"];
            _voiceId = configuration["ElevenLabs:VoiceId"] ?? "21m00Tcm4TlvDq8ikWAM"; // Default Rachel Voice
            _logger = logger;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
        }

        private async Task<string> GetActiveApiKeyAsync(CancellationToken cancellationToken = default)
        {
            var apiKey = _systemApiKey;
            var currentUserIntId = _currentUserService.UserId;
            if (currentUserIntId.HasValue)
            {
                var userId = currentUserIntId.Value;
                var user = await _unitOfWork.Users.GetQueryable()
                    .Include(u => u.InterviewSessions)
                    .Include(u => u.ApiKeys)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                
                if (user != null)
                {
                    bool hasFreeQuota = (userId == 2) || (user.InterviewSessions.Count == 0);
                    if (!hasFreeQuota)
                    {
                        if (user.ApiKeys == null || string.IsNullOrEmpty(user.ApiKeys.ElevenLabsApiKey))
                        {
                            throw new InvalidOperationException("Ücretsiz mülakat hakkınız doldu. Lütfen 'Ayarlar' sayfasından kendi ElevenLabs API Anahtarınızı sisteme girin.");
                        }
                        return _encryptionService.Decrypt(user.ApiKeys.ElevenLabsApiKey);
                    }
                }
            }
            return apiKey;
        }

        public async Task<Stream> TextToSpeechAsync(string text, CancellationToken cancellationToken = default)
        {
            var activeApiKey = await GetActiveApiKeyAsync(cancellationToken);
            if (string.IsNullOrEmpty(activeApiKey))
            {
                throw new InvalidOperationException("ElevenLabs API Key is missing.");
            }

            var url = $"https://api.elevenlabs.io/v1/text-to-speech/{_voiceId}";

            var requestBody = new
            {
                text = text,
                model_id = "eleven_multilingual_v2",
                voice_settings = new
                {
                    stability = 0.5,
                    similarity_boost = 0.5
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("xi-api-key", activeApiKey);
            request.Content = content;

            _logger.LogInformation("Sending TTS request to ElevenLabs API for VoiceId: {VoiceId}", _voiceId);
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("ElevenLabs TTS API Error. Status: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                throw new Exception($"ElevenLabs API Error: {response.StatusCode} - {errorContent}");
            }

            _logger.LogInformation("TTS request successful, returning audio stream.");
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        public async Task<string> SaveAudioAsync(Stream audioStream, string fileName, CancellationToken cancellationToken = default)
        {
            // wwwroot/audio/ai klasörüne kaydet
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio", "ai");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await audioStream.CopyToAsync(fileStream, cancellationToken);
            }

            // Web için relative path döndür
            return Path.Combine("audio", "ai", fileName).Replace("\\", "/");
        }

        public async Task<string> SpeechToTextAsync(Stream audioStream, CancellationToken cancellationToken = default)
        {
            var activeApiKey = await GetActiveApiKeyAsync(cancellationToken);
            if (string.IsNullOrEmpty(activeApiKey))
            {
                throw new InvalidOperationException("ElevenLabs API Key is missing.");
            }

            var url = "https://api.elevenlabs.io/v1/speech-to-text";
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(audioStream), "file", "audio.mp3");
            content.Add(new StringContent("scribe_v1"), "model_id");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("xi-api-key", activeApiKey);
            request.Content = content;

            _logger.LogInformation("Sending STT request to ElevenLabs API.");
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("ElevenLabs STT API Error. Status: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                throw new Exception($"ElevenLabs STT API Error: {response.StatusCode} - {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(responseString);
            var extractedText = document.RootElement.GetProperty("text").GetString() ?? string.Empty;
            _logger.LogInformation("STT request successful, extracted text length: {TextLength}", extractedText.Length);
            return extractedText;
        }
    }
}
