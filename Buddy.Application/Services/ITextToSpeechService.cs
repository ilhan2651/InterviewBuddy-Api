using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Services
{
    public interface ITextToSpeechService
    {
        Task<Stream> TextToSpeechAsync(string text, CancellationToken cancellationToken = default);
        Task<string> SaveAudioAsync(Stream audioStream, string fileName, CancellationToken cancellationToken = default);
        Task<string> SpeechToTextAsync(Stream audioStream, CancellationToken cancellationToken = default);
    }
}
