using Buddy.Application.Services;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Buddy.Infrastructure.Services.Gemini
{
    public class GeminiChatService : GeminiServiceBase, IChatLLMService
    {
        public GeminiChatService(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<string> TranscribeAudioAsync(Stream audioStream)
        {
            using var memoryStream = new MemoryStream();
            await audioStream.CopyToAsync(memoryStream);
            var audioBytes = memoryStream.ToArray();

            var prompt = "LÃ¼tfen bu ses kaydÄ±ndaki konuÅŸmayÄ± TÃ¼rkÃ§e olarak metne dÃ¶nÃ¼ÅŸtÃ¼r. Sadece konuÅŸulan metni yaz, baÅŸka aÃ§Ä±klama ekleme.";

            var model = GoogleAI.GenerativeModel(model: ModelName);
            var response = await model.GenerateContent(prompt);
            return response?.Text ?? string.Empty;
        }

        public async Task<string> GenerateChatResponseAsync(string userMessage, List<Message> history)
        {
            var model = GoogleAI.GenerativeModel(model: ModelName);

            var systemInstruction = "Sen 'StudyBuddy' adÄ±nda yardÄ±mcÄ± bir eÄŸitim asistanÄ±sÄ±n. Ã–ÄŸrencilere derslerinde yardÄ±mcÄ± olur, sorularÄ±nÄ± yanÄ±tlar ve onlarÄ± motive edersin. YanÄ±tlarÄ±n kÄ±sa, Ã¶z ve teÅŸvik edici olmalÄ±dÄ±r.";

            var fullPrompt = new StringBuilder();
            fullPrompt.AppendLine(systemInstruction);
            fullPrompt.AppendLine();

            foreach (var msg in history)
            {
                var role = msg.Type == MessageType.User ? "KullanÄ±cÄ±" : "Asistan";
                fullPrompt.AppendLine($"{role}: {msg.TextContent}");
            }

            fullPrompt.AppendLine($"KullanÄ±cÄ±: {userMessage}");
            fullPrompt.AppendLine("Asistan:");

            var response = await model.GenerateContent(fullPrompt.ToString());
            return response?.Text ?? string.Empty;
        }

        public Task<Stream> TextToSpeechAsync(string text)
        {
            throw new System.NotImplementedException("Gemini API currently does not support Text-to-Speech. Consider using Google Cloud Text-to-Speech API separately.");
        }
    }
}
