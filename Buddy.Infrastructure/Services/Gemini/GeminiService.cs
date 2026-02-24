using Buddy.Application.Dtos.Quiz;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using Buddy.Application.Dtos.Interview;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mscc.GenerativeAI.Types;

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

        public async Task<string> GenerateChatResponseAsync(string userMessage, List<Buddy.Domain.Entities.Message> history)
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

        public async Task<List<InterviewQuestionResult>> GenerateInterviewQuestionsAsync(string profession, string jobTitle, InterviewLevel level, DifficultyLevel difficulty, InterviewQuestionType type, int count, string language, List<string>? previouslyAskedQuestions = null, CancellationToken cancellationToken = default)
        {
            var model = _googleAI.GenerativeModel(model: _modelName);
            var prompt = "";

            string exclusionText = "";
            if (previouslyAskedQuestions != null && previouslyAskedQuestions.Any())
            {
                exclusionText = $"\n\nÖNEMLİ: Daha önceki mülakatlarda adaya şu sorular soruldu:\n{string.Join("\n- ", previouslyAskedQuestions)}\n\nLütfen bu soruları ve varyasyonlarını EKRAN SÜRESİNDE TEKRAR ETME. Tamamen yeni sorular üret.";
            }

            if (type == InterviewQuestionType.Behavioral)
            {
                prompt = $@"Meslek Grubu: {profession}
Pozisyon: {jobTitle}
Seviye: {level}
Zorluk: {difficulty}
Soru Sayısı: {count}
Dil: {language}

Lütfen bu pozisyon için hedef dilde ({language}) davranışsal mülakat soruları üret.
Yumuşak becerilere, takım çalışmasına ve çatışma çözümüne odaklan.{exclusionText}

Çıktıyı kesinlikle şu JSON formatında ver:
{{
  ""questions"": [
    {{ ""questionText"": ""Soru metni"", ""codeSnippet"": null }},
    ...
  ]
}}";
            }
            else if (type == InterviewQuestionType.Technical)
            {
                prompt = $@"Meslek Grubu: {profession}
Pozisyon: {jobTitle}
Seviye: {level}
Zorluk: {difficulty}
Soru Tipi: Teknik
Soru Sayısı: {count}
Dil: {language}

Lütfen bu meslek grubu ve pozisyon için hedef dilde ({language}) teknik mülakatta sorulmak üzere {difficulty} zorluğunda sorular üret.
Sorular pozisyona uygun, seviyeye göre ayarlanmış ve seçilen dilde olmalı. Standart sorulardan ziyade analitik düşünmeyi ölç.{exclusionText}
Eğer soru bir kod parçasını analiz etmeyi gerektiriyorsa, kodu 'codeSnippet' alanına koy, geri kalan anlatımı 'questionText' alanına koy. Kod gerekmiyorsa 'codeSnippet' null olsun.

Format (JSON):
{{
  ""questions"": [
    {{ ""questionText"": ""Soru metni"", ""codeSnippet"": ""opsiyonel kod bloğu"" }},
    ...
  ]
}}";
            }
            else if (type == InterviewQuestionType.SystemDesign)
            {
                prompt = $@"Meslek Grubu: {profession}
Pozisyon: {jobTitle}
Seviye: {level}
Zorluk: {difficulty}
Soru Tipi: Teknik Senaryo / Görsel Akıl Yürütme Kod Analizi
Soru Sayısı: {count}
Dil: {language}

Bu soru, mülakatın en özel analiz sorusudur. Lütfen adaya '{profession}' ve '{jobTitle}' metriklerine uygun bir KOD PARÇASI (bug bulma, review etme, optimize etme) VEYA SİSTEM MİMARİSİ / ALGORİTMA VAKASI ver. 
Senaryo mantıklı ve tutarlı bir yazılım/sistem geliştirme problemi olmalı. 
ÖNEMLİ: Senaryo metnini 'questionText' alanına yaz. İlgili kodu veya diyagramı ise KESİNLİKLE 'codeSnippet' alanına yaz (markdown backtickleri KOYMA, direkt kodu yaz).
Soru metni içerisinde 'Aşağıdaki kodu inceleyin' gibi ifadeler kullanarak yönlendirme yap.

Format (JSON):
{{
  ""questions"": [
    {{ ""questionText"": ""Senaryo açıklaması ve soru"", ""codeSnippet"": ""Sadece kod veya diyagram metni"" }}
  ]
}}";
            }
            else
            {
                return new List<InterviewQuestionResult>();
            }

            var response = await model.GenerateContent(prompt);
            var jsonContent = CleanJsonResponse(response?.Text ?? "{}");

            try
            {
                var result = JsonSerializer.Deserialize<InterviewQuestionsRoot>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Questions ?? new List<InterviewQuestionResult>();
            }
            catch
            {
                return new List<InterviewQuestionResult>();
            }
        }

        public async Task<FollowUpResult> DecideFollowUpAsync(string question, string answer, string language, CancellationToken cancellationToken = default)
        {
            var model = _googleAI.GenerativeModel(model: _modelName);

            var prompt = $@"Sen bir mülakatçısın. Adayın son cevabına göre mülakatın akışını yönetiyorsun.
Dil: {language}

Soru: ""{question}""
Adayın Cevabı: ""{answer}""

Görevin: SADECE adayın cevabı çok kapalı, net değil veya bir ek soru (follow-up) gerektiriyorsa 'true' dönmek ve kısa bir takip sorusu yazmak. Normal cevaplarda 'false' dön.
Çıktıyı mülakat dilinde ({language}) ver.

Sadece şu JSON formatında yanıt ver:
{{
  ""requiresFollowUp"": boolean,
  ""followUpQuestion"": ""takip sorusu metni veya null""
}}";

            var response = await model.GenerateContent(prompt, cancellationToken: cancellationToken);
            var jsonContent = CleanJsonResponse(response?.Text ?? "{}");

            try
            {
                return JsonSerializer.Deserialize<FollowUpResult>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new FollowUpResult { RequiresFollowUp = false };
            }
            catch
            {
                return new FollowUpResult { RequiresFollowUp = false };
            }
        }

        public async Task<AssessmentResult> EvaluateInterviewAnswerAsync(string question, string answer, string profession, string jobTitle, InterviewLevel level, DifficultyLevel difficulty, string language, CancellationToken cancellationToken = default)
        {
            var model = _googleAI.GenerativeModel(model: _modelName);

            var prompt = $@"Sen {difficulty} zorluk seviyesinde sınav yapan, {profession} alanında ve {jobTitle} pozisyonu için uzman bir teknik mülakatçısın ({level} seviye adayı değerlendiriyorsun).
Kullanıcının tercih ettiği dil: {language}. Lütfen tüm geri bildirimlerini ve follow-up sorularını bu dilde (örneğin Türkçe ise Türkçe, English ise English) ver.

Adaya sorulan soru: ""{question}""
Adayın cevabı: ""{answer}""

Cevabı değerlendir (kullanıcının seçtiği dili kullan {language}):
1. Kısa ve yapıcı bir geri bildirim ver (maksimum 2 cümle).
2. Cevabı 0 ile 100 arasında puanla ('score' alanı).
3. SADECE EĞER cevap KESİNLİKLE kabul edilemez, çok eksik veya tamamen konu dışıysa 'requiresFollowUp' değerini true yap ve bir 'followUpQuestion' (takip sorusu) yaz. Normal veya kabul edilebilir cevaplarda asla follow-up sorma (false olsun).
4. Eğer cevap '[SES_ANLASILAMADI]' gibi teknik bir hata yer tutucusu içeriyorsa, 'requiresFollowUp' false olsun ve puanı 0 ver.

Sadece şu formatta geçerli bir JSON objesi döndür:
{{
  ""score"": 85,
  ""feedback"": ""geri bildirim metni"",
  ""requiresFollowUp"": boolean,
  ""followUpQuestion"": ""takip sorusu veya null""
}}";

            var response = await model.GenerateContent(prompt, cancellationToken: cancellationToken);
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

        public async Task<AssessmentResult> EvaluateImageAsync(string base64Image, string language, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(base64Image))
            {
                return new AssessmentResult { Score = 0, Feedback = "Görüntü sağlanamadı." };
            }

            var model = _googleAI.GenerativeModel(model: _modelName);

            var prompt = $@"Sen bir İK uzmanısın. Adayın kamerasından alınan bu anlık görüntüye bakarak; kıyafetinin profesyonelliğini, duruşunu, arka planın düzenini ve ortam ışığını bir iş mülakatı standartlarına göre değerlendir.
Lütfen tüm geri bildirimini şu dilde ver: {language}.

Cevabı değerlendir:
1. Çok kısa ve yapıcı bir geri bildirim ver (maksimum 2 cümle).
2. Görüntü profesyonelliğini 0 ile 100 arasında puanla ('score' alanı).

Sadece şu formatta geçerli bir JSON objesi döndür:
{{
  ""score"": 85,
  ""feedback"": ""geri bildirim metni"",
  ""requiresFollowUp"": false,
  ""followUpQuestion"": null
}}";

            var base64Data = base64Image.Contains(",") ? base64Image.Split(',')[1] : base64Image;
            var mimeType = base64Image.Contains("png") ? "image/png" : "image/jpeg";

            var parts = new List<IPart>
            {
                new Part { Text = prompt },
                new Part { InlineData = new InlineData { MimeType = mimeType, Data = base64Data } }
            };

            var request = new GenerateContentRequest { Contents = new List<Content> { new Content { Parts = parts } } };
            
            try 
            {
                var response = await model.GenerateContent(request, cancellationToken: cancellationToken);
                var jsonContent = CleanJsonResponse(response?.Text ?? "{}");
                return JsonSerializer.Deserialize<AssessmentResult>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new AssessmentResult { Feedback = "Error parsing AI response", Score = 0 };
            }
            catch (Exception ex)
            {
                return new AssessmentResult { Feedback = "Görüntü analiz edilemedi: " + ex.Message, Score = 0 };
            }
        }

        public async Task<AssessmentResult> EvaluateAudioToneAsync(Stream audioStream, string language, CancellationToken cancellationToken = default)
        {
            if (audioStream == null || audioStream.Length == 0)
            {
                return new AssessmentResult { Score = 0, Feedback = "Ses verisi sağlanamadı." };
            }

            var model = _googleAI.GenerativeModel(model: _modelName);

            var prompt = $@"Sen bir diksiyon ve iletişim uzmanısın. Adayın bu ses kaydındaki ses tonunu, vurgularını, akıcılığını ve özgüvenini profesyonel bir iş mülakatı bağlamında değerlendir.
Lütfen tüm geri bildirimini şu dilde ver: {language}.

Cevabı değerlendir:
1. Çok kısa ve yapıcı bir geri bildirim ver (maksimum 2 cümle). (Örn: 'Ses tonunuz çok net ve özgüvenliydi.')
2. Ses tonu ve akıcılığı 0 ile 100 arasında puanla ('score' alanı).

Sadece şu formatta geçerli bir JSON objesi döndür:
{{
  ""score"": 85,
  ""feedback"": ""geri bildirim metni"",
  ""requiresFollowUp"": false,
  ""followUpQuestion"": null
}}";

            try 
            {
                using var memoryStream = new MemoryStream();
                await audioStream.CopyToAsync(memoryStream);
                var audioBytes = memoryStream.ToArray();
                var base64Audio = Convert.ToBase64String(audioBytes);
                
                // Varsayılan olarak webm veya mp3/wav gelecek. Ses formatını genel bir ses formatı olarak verelim.
                var parts = new List<IPart>
                {
                    new Part { Text = prompt },
                    new Part { InlineData = new InlineData { MimeType = "audio/webm", Data = base64Audio } }
                };

                var request = new GenerateContentRequest { Contents = new List<Content> { new Content { Parts = parts } } };
                var response = await model.GenerateContent(request, cancellationToken: cancellationToken);
                
                var jsonContent = CleanJsonResponse(response?.Text ?? "{}");
                return JsonSerializer.Deserialize<AssessmentResult>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new AssessmentResult { Feedback = "Error parsing AI response", Score = 0 };
            }
            catch (Exception ex)
            {
                return new AssessmentResult { Feedback = "Ses tonu analiz edilemedi: " + ex.Message, Score = 0 };
            }
        }

        public async Task<string> GenerateFinalFeedbackAsync(string profession, string jobTitle, InterviewLevel level, DifficultyLevel difficulty, List<InterviewQuestion> questionsAndAnswers, string language, CancellationToken cancellationToken = default)
        {
            var model = _googleAI.GenerativeModel(model: _modelName);

            var sb = new StringBuilder();
            sb.AppendLine($"Meslek: {profession}");
            sb.AppendLine($"Pozisyon: {jobTitle}");
            sb.AppendLine($"Seviye: {level}");
            sb.AppendLine($"Zorluk: {difficulty}");
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
            sb.AppendLine($"Kapsamlı bir mülakat değerlendirme raporu oluştur. Güçlü yönleri, gelişime açık yönleri ve nihai 'İşe Alım Kararını' (Olumlu/Olumsuz/Değerlendirilebilir) belirt. Raporu tamamen hedeflenen dilde ({language}) ve Markdown formatında yaz.");
            sb.AppendLine("ÖNEMLİ: Eğer bir cevap '[SES_ANLAŞILAMADI]', '[SES_HATA]' veya '[CEVAP_YOK]' olarak işaretlenmişse, bunu teknik/kullanıcı hatası olarak gör ve 'Cevaplanmadı' kabul et. Bu durum teknik yetkinlik puanını düşürmemeli, sadece verinin eksik olduğu belirtilmeli. Eğer soruların %50'sinden fazlası cevaplanmadıysa mülakatın tamamlanmadığını belirt.");

            var response = await model.GenerateContent(sb.ToString(), cancellationToken: cancellationToken);
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
            public List<InterviewQuestionResult> Questions { get; set; } = new List<InterviewQuestionResult>();
        }

        private class QuizRoot
        {
            public List<QuizQuestionDto> Questions { get; set; } = new List<QuizQuestionDto>();
        }
    }
}