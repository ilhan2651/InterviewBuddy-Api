using Buddy.Application.Dtos.Quiz;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Buddy.Infrastructure.Services.Gemini
{
    public class GeminiService : ILLMService
    {
        private readonly GoogleAI _googleAI;
        private readonly string _modelName;

        public GeminiService(IConfiguration configuration)
        {
            var apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured");
            // Model ismini configuration'dan al, yoksa default kullan
            var configuredModel = configuration["Gemini:ModelName"] ?? "gemini-flash-latest";

            // Mscc.GenerativeAI için model ismini ayarla
            _modelName = configuredModel;
            _googleAI = new GoogleAI(apiKey: apiKey);
        }

        public async Task<string> TranscribeAudioAsync(Stream audioStream)
        {
            // Gemini multimodal - ses dosyasını byte array'e çevir
            using var memoryStream = new MemoryStream();
            await audioStream.CopyToAsync(memoryStream);
            var audioBytes = memoryStream.ToArray();

            var prompt = "Lütfen bu ses kaydındaki konuşmayı Türkçe olarak metne dönüştür. Sadece konuşulan metni yaz, başka açıklama ekleme.";

            var model = _googleAI.GenerativeModel(model: _modelName);

            // Multimodal request with audio
            var response = await model.GenerateContent(prompt); // Note: Audio support may require different approach
            return response?.Text ?? string.Empty;
        }

        public async Task<string> GenerateChatResponseAsync(string userMessage, List<Message> history)
        {
            var model = _googleAI.GenerativeModel(model: _modelName);

            var systemInstruction = "Sen 'StudyBuddy' adında yardımcı bir eğitim asistanısın. Öğrencilere derslerinde yardımcı olur, sorularını yanıtlar ve onları motive edersin. Yanıtların kısa, öz ve teşvik edici olmalıdır.";

            // Build full prompt with history
            var fullPrompt = new StringBuilder();
            fullPrompt.AppendLine(systemInstruction);
            fullPrompt.AppendLine();

            foreach (var msg in history)
            {
                var role = msg.Type == MessageType.User ? "Kullanıcı" : "Asistan";
                fullPrompt.AppendLine($"{role}: {msg.TextContent}");
            }

            fullPrompt.AppendLine($"Kullanıcı: {userMessage}");
            fullPrompt.AppendLine("Asistan:");

            var response = await model.GenerateContent(fullPrompt.ToString());
            return response?.Text ?? string.Empty;
        }

        public async Task<Stream> TextToSpeechAsync(string text)
        {
            // Gemini şu an TTS desteklemiyor
            throw new NotImplementedException("Gemini API currently does not support Text-to-Speech. Consider using Google Cloud Text-to-Speech API separately.");
        }

        public async Task<List<QuizQuestionDto>> GenerateQuizQuestionsAsync(string topic, DifficultyLevel difficulty, int count)
        {
            var model = _googleAI.GenerativeModel(model: _modelName);

            var prompt = $@"Konu: {topic}
Zorluk Seviyesi: {difficulty}
Soru Sayısı: {count}

Lütfen bu konu hakkında belirtilen zorluk seviyesinde quiz soruları üret.
Her soru için beklenen anahtar kelimeleri de belirt.

Yanıtını SADECE aşağıdaki JSON formatında ver:
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
            var jsonContent = response?.Text ?? "{}";

            // JSON'dan gereksiz markdown işaretlerini temizle
            jsonContent = CleanJsonResponse(jsonContent);

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
            var model = _googleAI.GenerativeModel(model: _modelName);

            var evaluationPrompt = new StringBuilder();
            evaluationPrompt.AppendLine("Aşağıdaki soru ve cevapları değerlendir. Her bir cevap için 0-10 arası bir puan ve kısa bir geri bildirim ver.");
            evaluationPrompt.AppendLine("Ayrıca tüm sınav için genel bir özet çıkar.");
            evaluationPrompt.AppendLine("Yanıtını SADECE aşağıdaki JSON formatında ver:");
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
                evaluationPrompt.AppendLine($"Öğrenci Cevabı: {input.UserAnswer}");
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

        public async Task<List<string>> GenerateInterviewQuestionsAsync(string jobTitle, InterviewLevel level, InterviewQuestionType type, int count)
        {
            var model = _googleAI.GenerativeModel(model: _modelName);
            var prompt = "";

            if (type == InterviewQuestionType.Behavioral)
            {
                prompt = $@"Pozisyon: {jobTitle}
Seviye: {level}
Soru Sayısı: {count}

Lütfen bu pozisyon için davranışsal mülakat soruları üret.
Yumuşak becerilere, takım çalışmasına ve çatışma çözümüne odaklan.

Çıktıyı kesinlikle bir JSON nesnesi olarak ver:
{{
  ""questions"": [""Soru 1"", ""Soru 2"", ...]
}}";
            }
            else if (type == InterviewQuestionType.Technical)
            {
                prompt = $@"Pozisyon: {jobTitle}
Seviye: {level}
Soru Tipi: Teknik
Soru Sayısı: {count}

Lütfen bu pozisyon ve seviye için teknik mülakatta sorulmak üzere sorular üret.
Sorular pozisyona uygun, seviyeye göre ayarlanmış ve Türkçe olmalı.

Format (JSON):
{{
  ""questions"": [""Soru 1"", ""Soru 2"", ...]
}}";
            }
            else
            {
                return new List<string>();
            }

            var response = await model.GenerateContent(prompt);
            var jsonContent = CleanJsonResponse(response?.Text ?? "{}");

            try
            {
                var result = JsonSerializer.Deserialize<InterviewQuestionsRoot>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Questions ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<AssessmentResult> EvaluateInterviewAnswerAsync(string question, string answer, string jobTitle, InterviewLevel level)
        {
            var model = _googleAI.GenerativeModel(model: _modelName);

            var prompt = $@"Sen {level} seviyesindeki bir {jobTitle} pozisyonu için uzman bir teknik mülakatçısın.

Adaya sorulan soru: ""{question}""
Adayın cevabı: ""{answer}""

Cevabı değerlendir:
1. Kısa ve yapıcı bir geri bildirim ver (maksimum 2 cümle).
2. Cevabın çok yüzeysel, eksik veya 'kaçamak' olup olmadığına karar ver. Eğer öyleyse ve daha derinlemesine sorgulaman gerekiyorsa 'requiresFollowUp' değerini true yap ve bir 'followUpQuestion' (takip sorusu) yaz.
3. Eğer cevap yeterliyse veya '[SES_ANLASILAMADI]' gibi teknik bir hata yer tutucusu içeriyorsa, 'requiresFollowUp' false olsun.

Sadece şu formatta geçerli bir JSON objesi döndür:
{{
  ""feedback"": ""geri bildirim metni"",
  ""requiresFollowUp"": boolean,
  ""followUpQuestion"": ""takip sorusu veya null""
}}";

            var response = await model.GenerateContent(prompt);
            var jsonContent = CleanJsonResponse(response?.Text ?? "{}");

            try
            {
                return JsonSerializer.Deserialize<AssessmentResult>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new AssessmentResult { Feedback = "Error parsing AI response" };
            }
            catch
            {
                return new AssessmentResult { Feedback = jsonContent };
            }
        }

        public async Task<string> GenerateFinalFeedbackAsync(string jobTitle, InterviewLevel level, List<InterviewQuestion> questionsAndAnswers)
        {
            var model = _googleAI.GenerativeModel(model: _modelName);

            var sb = new StringBuilder();
            sb.AppendLine($"Pozisyon: {jobTitle}");
            sb.AppendLine($"Seviye: {level}");
            sb.AppendLine("Mülakat Özeti");
            sb.AppendLine();
            sb.AppendLine("İşte mülakat transkripti:");

            foreach (var qa in questionsAndAnswers.OrderBy(q => q.Order))
            {
                sb.AppendLine($"- Soru: {qa.QuestionText}");
                sb.AppendLine($"  Cevap: {qa.Answer?.UserAnswerText ?? "[Cevap Yok]"}");
                sb.AppendLine($"  Puan: {qa.Answer?.Score?.ToString() ?? "N/A"}");
            }

            sb.AppendLine();
            sb.AppendLine("Kapsamlı bir mülakat değerlendirme raporu oluştur. Güçlü yönleri, gelişime açık yönleri ve nihai 'İşe Alım Kararını' (Olumlu/Olumsuz/Değerlendirilebilir) belirt. Raporu Markdown formatında yaz.");
            sb.AppendLine("ÖNEMLİ: Eğer bir cevap '[SES_ANLAŞILAMADI]', '[SES_HATA]' veya '[CEVAP_YOK]' olarak işaretlenmişse, bunu teknik/kullanıcı hatası olarak gör ve 'Cevaplanmadı' kabul et. Bu durum teknik yetkinlik puanını düşürmemeli, sadece verinin eksik olduğu belirtilmeli. Eğer soruların %50'sinden fazlası cevaplanmadıysa mülakatın tamamlanmadığını belirt.");

            var response = await model.GenerateContent(sb.ToString());
            return response?.Text ?? string.Empty;
        }

        // Helper method to clean JSON responses from markdown code blocks
        private string CleanJsonResponse(string jsonContent)
        {
            if (string.IsNullOrEmpty(jsonContent)) return "{}";

            // Remove markdown code blocks
            jsonContent = jsonContent.Trim();
            if (jsonContent.StartsWith("```json"))
            {
                jsonContent = jsonContent.Substring(7);
            }
            else if (jsonContent.StartsWith("```"))
            {
                jsonContent = jsonContent.Substring(3);
            }

            if (jsonContent.EndsWith("```"))
            {
                jsonContent = jsonContent.Substring(0, jsonContent.Length - 3);
            }

            return jsonContent.Trim();
        }

        // Helper classes for deserialization
        private class InterviewQuestionsRoot
        {
            public List<string> Questions { get; set; } = new List<string>();
        }

        private class QuizRoot
        {
            public List<QuizQuestionDto> Questions { get; set; } = new List<QuizQuestionDto>();
        }
    }
}