using Buddy.Application.Dtos.Quiz;
using Buddy.Application.Services;
using Buddy.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Buddy.Infrastructure.Services.Gemini
{
    public class GeminiQuizService : GeminiServiceBase, IQuizLLMService
    {
        public GeminiQuizService(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<QuizQuestionDto>> GenerateQuizQuestionsAsync(string topic, DifficultyLevel difficulty, int count)
        {
            var model = GoogleAI.GenerativeModel(model: ModelName);

            var prompt = $@"Konu: {topic}
Zorluk Seviyesi: {difficulty}
Soru SayÄ±sÄ±: {count}

LÃ¼tfen bu konu hakkÄ±nda belirtilen zorluk seviyesinde quiz sorularÄ± Ã¼ret.
Her soru iÃ§in beklenen anahtar kelimeleri de belirt.

YanÄ±tÄ±nÄ± SADECE aÅŸaÄŸÄ±daki JSON formatÄ±nda ver:
{{
  ""questions"": [
    {{
      ""number"": 1,
      ""question"": ""Soru metni buraya"",
      ""expectedKeywords"": [""anahtar1"", ""anahtar2"", ""anahtar3""]
    }}
  ]
}}";

            var response = await model.GenerateContent(prompt);
            var jsonContent = CleanJsonResponse(response?.Text ?? "{}");

            try
            {
                var result = JsonSerializer.Deserialize<QuizRoot>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Questions ?? new List<QuizQuestionDto>();
            }
            catch
            {
                return new List<QuizQuestionDto>();
            }
        }

        public async Task<QuizEvaluationDto> EvaluateQuizAsync(List<QuizEvaluationInput> inputs)
        {
            var model = GoogleAI.GenerativeModel(model: ModelName);

            var evaluationPrompt = new StringBuilder();
            evaluationPrompt.AppendLine("AÅŸaÄŸÄ±daki soru ve cevaplarÄ± deÄŸerlendir. Her bir cevap iÃ§in 0-10 arasÄ± bir puan ve kÄ±sa bir geri bildirim ver.");
            evaluationPrompt.AppendLine("AyrÄ±ca tÃ¼m sÄ±nav iÃ§in genel bir Ã¶zet Ã§Ä±kar.");
            evaluationPrompt.AppendLine("YanÄ±tÄ±nÄ± SADECE aÅŸaÄŸÄ±daki JSON formatÄ±nda ver:");
            evaluationPrompt.AppendLine("{");
            evaluationPrompt.AppendLine("  \"evaluations\": [");
            evaluationPrompt.AppendLine("    { \"questionNumber\": 1, \"score\": 8.5, \"feedback\": \"...\" }");
            evaluationPrompt.AppendLine("  ],");
            evaluationPrompt.AppendLine("  \"totalScore\": 8.5,");
            evaluationPrompt.AppendLine("  \"summary\": \"...\"");
            evaluationPrompt.AppendLine("}");
            evaluationPrompt.AppendLine();
            evaluationPrompt.AppendLine("Sorular ve Cevaplar:");

            foreach (var input in inputs)
            {
                evaluationPrompt.AppendLine($"Soru {input.QuestionNumber}: {input.Question}");
                evaluationPrompt.AppendLine($"Anahtar Kelimeler: {string.Join(", ", input.ExpectedKeywords)}");
                evaluationPrompt.AppendLine($"Ã–ÄŸrenci CevabÄ±: {input.UserAnswer}");
                evaluationPrompt.AppendLine("---");
            }

            var response = await model.GenerateContent(evaluationPrompt.ToString());
            var jsonContent = CleanJsonResponse(response?.Text ?? "{}");

            try
            {
                var result = JsonSerializer.Deserialize<QuizEvaluationDto>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new QuizEvaluationDto();
            }
            catch
            {
                return new QuizEvaluationDto();
            }
        }

        private class QuizRoot
        {
            public List<QuizQuestionDto> Questions { get; set; } = new();
        }
    }
}
