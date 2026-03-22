using Buddy.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Buddy.Application.Services
{
    public interface IChatLLMService
    {
        Task<string> TranscribeAudioAsync(Stream audioStream);
        Task<string> GenerateChatResponseAsync(string userMessage, List<Message> history);
        Task<Stream> TextToSpeechAsync(string text);
    }
}
