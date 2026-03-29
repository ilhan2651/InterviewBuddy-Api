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
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Linq;

namespace Buddy.Infrastructure.Services.ElevenLabs
{
    public class ElevenLabsService : ITextToSpeechService
    {
        private readonly HttpClient _httpClient;
        private readonly string _systemApiKey;
        private readonly string _voiceId;
        private readonly ILogger<ElevenLabsService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;
        private readonly IApiKeyValidationService _apiKeyValidationService;
        private readonly string _audioRootPath;

        public ElevenLabsService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ElevenLabsService> logger,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            IEncryptionService encryptionService,
            IApiKeyValidationService apiKeyValidationService)
        {
            _httpClient = httpClient;
            _systemApiKey = configuration["ElevenLabs:ApiKey"] ?? string.Empty;
            _voiceId = configuration["ElevenLabs:VoiceId"] ?? "21m00Tcm4TlvDq8ikWAM";
            _logger = logger;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _apiKeyValidationService = apiKeyValidationService;
            _audioRootPath = ResolveAudioRootPath(configuration);
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
                    var completedInterviewCount = user.InterviewSessions.Count(s => s.CompletedAt.HasValue);
                    var hasFreeQuota = completedInterviewCount == 0;

                    if (!hasFreeQuota)
                    {
                        if (user.ApiKeys == null || string.IsNullOrEmpty(user.ApiKeys.ElevenLabsApiKey))
                        {
                            throw new InvalidOperationException("ElevenLabs kredisi bitti veya anahtar yok.");
                        }

                        var decryptedApiKey = _encryptionService.Decrypt(user.ApiKeys.ElevenLabsApiKey);
                        var (isValid, _) = await _apiKeyValidationService.ValidateElevenLabsKeyAsync(decryptedApiKey, cancellationToken);
                        if (!isValid)
                        {
                            throw new ArgumentException("ElevenLabs API anahtarı geçersiz.");
                        }

                        return decryptedApiKey;
                    }
                }
            }

            return apiKey;
        }

        public async Task<Stream> TextToSpeechAsync(string text, string language = "Turkish", CancellationToken cancellationToken = default)
        {
            try
            {
                var activeApiKey = await GetActiveApiKeyAsync(cancellationToken);
                if (string.IsNullOrEmpty(activeApiKey))
                {
                    throw new InvalidOperationException("ElevenLabs API Key is missing.");
                }

                var url = $"https://api.elevenlabs.io/v1/text-to-speech/{_voiceId}";
                var requestBody = new
                {
                    text,
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
                    throw new Exception("ElevenLabs API failed with status " + response.StatusCode);
                }

                _logger.LogInformation("ElevenLabs TTS request successful.");
                return await response.Content.ReadAsStreamAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ElevenLabs TTS failed or quota expired. Falling back to Free Edge TTS.");
                return await TextToSpeechEdgeAsync(text, language, cancellationToken);
            }
        }

        private async Task<Stream> TextToSpeechEdgeAsync(string text, string language, CancellationToken cancellationToken)
        {
            var isEnglish = language != null && (language.Equals("English", StringComparison.OrdinalIgnoreCase) || language.Equals("İngilizce", StringComparison.OrdinalIgnoreCase));
            var voiceName = isEnglish ? "en-US-AriaNeural" : "tr-TR-EmelNeural";
            
            var tempFile = Path.GetTempFileName() + ".mp3";
            var escapedText = text.Replace("\"", "\\\"");

            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"-m edge_tts --text \"{escapedText}\" --voice \"{voiceName}\" --write-media \"{tempFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = System.Diagnostics.Process.Start(processStartInfo);
            if (process == null) throw new Exception("Failed to start python edge_tts process.");

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                throw new Exception($"edge-tts failed with exit code {process.ExitCode}: {error}");
            }

            var memoryStream = new MemoryStream();
            using (var fs = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
            {
                await fs.CopyToAsync(memoryStream, cancellationToken);
            }
            
            try { File.Delete(tempFile); } catch { /* ignore */ }

            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task<string> SaveAudioAsync(Stream audioStream, string fileName, CancellationToken cancellationToken = default)
        {
            var folderPath = Path.Combine(_audioRootPath, "ai");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await audioStream.CopyToAsync(fileStream, cancellationToken);
                await fileStream.FlushAsync(cancellationToken);
            }

            return Path.Combine("audio", "ai", fileName).Replace("\\", "/");
        }

        public async Task<string> SpeechToTextAsync(Stream audioStream, CancellationToken cancellationToken = default)
        {
            try
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

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("ElevenLabs STT failed");
                }

                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                using var document = JsonDocument.Parse(responseString);
                return document.RootElement.GetProperty("text").GetString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ElevenLabs STT Failed. Expecting Web Speech API fallback result from client.");
                return "[SES_HATA]";
            }
        }

        private string ResolveAudioRootPath(IConfiguration configuration)
        {
            var configuredRoot = configuration["AudioStorage:RootPath"];
            if (!string.IsNullOrWhiteSpace(configuredRoot)) return configuredRoot;
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
        }
    }
}
