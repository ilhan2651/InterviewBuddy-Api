using Buddy.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Infrastructure.Services.FreeTts
{
    public class FreeGoogleTtsService : ITextToSpeechService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FreeGoogleTtsService> _logger;
        private readonly string _audioRootPath;

        public FreeGoogleTtsService(HttpClient httpClient, IConfiguration configuration, ILogger<FreeGoogleTtsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _audioRootPath = ResolveAudioRootPath(configuration);
        }

        public async Task<Stream> TextToSpeechAsync(string text, string language = "Turkish", CancellationToken cancellationToken = default)
        {
            var outputStream = new MemoryStream();
            
            // Chunking by lengths of ~150 chars
            var chunks = ChunkText(text, 150);

            var isEnglish = language != null && (language.Equals("English", StringComparison.OrdinalIgnoreCase) || language.Equals("İngilizce", StringComparison.OrdinalIgnoreCase));
            var langCode = isEnglish ? "en" : "tr";

            foreach (var chunk in chunks)
            {
                if (string.IsNullOrWhiteSpace(chunk)) continue;

                var encoded = Uri.EscapeDataString(chunk.Trim());
                // Google Translate TTS endpoint (client=tw-ob is fully free). This naturally defaults to a Female Voice in both EN and TR!
                var url = $"https://translate.google.com/translate_tts?ie=UTF-8&tl={langCode}&client=tw-ob&q={encoded}";
                
                try 
                {
                    _logger.LogInformation("Fetching Free Google TTS chunk: {ChunkLength} chars, Lang: {LangCode}", chunk.Length, langCode);
                    var response = await _httpClient.GetAsync(url, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    var chunkBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                    await outputStream.WriteAsync(chunkBytes, 0, chunkBytes.Length, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching Free Google TTS chunk.");
                }
            }

            outputStream.Position = 0;
            return outputStream;
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

        public Task<string> SpeechToTextAsync(Stream audioStream, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Free STT bypass engaged.");
            return Task.FromResult("[SES_HATA]");
        }

        private List<string> ChunkText(string text, int maxLength)
        {
            var words = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<string>();
            var currentChunk = "";

            foreach (var word in words)
            {
                if (currentChunk.Length + word.Length + 1 > maxLength)
                {
                    chunks.Add(currentChunk);
                    currentChunk = word;
                }
                else
                {
                    currentChunk = string.IsNullOrEmpty(currentChunk) ? word : currentChunk + " " + word;
                }
            }
            if (!string.IsNullOrEmpty(currentChunk))
            {
                chunks.Add(currentChunk);
            }
            return chunks;
        }

        private string ResolveAudioRootPath(IConfiguration configuration)
        {
            var configuredRoot = configuration["AudioStorage:RootPath"];
            if (!string.IsNullOrWhiteSpace(configuredRoot)) return configuredRoot;
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
        }
    }
}
