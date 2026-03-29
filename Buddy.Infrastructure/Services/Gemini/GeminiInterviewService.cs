using Buddy.Application.Dtos.Interview;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Infrastructure.Services.Gemini
{
    public class GeminiInterviewService : GeminiServiceBase, IInterviewLLMService
    {
        public GeminiInterviewService(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<InterviewQuestionResult>> GenerateInterviewQuestionsAsync(
            string profession,
            string jobTitle,
            InterviewLevel level,
            DifficultyLevel difficulty,
            InterviewQuestionType type,
            int count,
            string language,
            List<string>? previouslyAskedQuestions = null,
            CancellationToken cancellationToken = default)
        {
            var model = GoogleAI.GenerativeModel(model: ModelName);
            var prompt = string.Empty;
            
            var matrixGuidance = PromptMatrix.GetGuidance(profession, jobTitle, level, difficulty);

            var exclusionText = string.Empty;
            if (previouslyAskedQuestions != null && previouslyAskedQuestions.Any())
            {
                exclusionText =
                    $"\n\nONEMLI: Daha onceki mulakatlarda adaya su sorular soruldu:\n- {string.Join("\n- ", previouslyAskedQuestions)}\n\nLutfen bu sorulari ve varyasyonlarini tekrar etme. Tamamen yeni sorular uret.";
            }

            if (type == InterviewQuestionType.Behavioral)
            {
                prompt = $@"Meslek Grubu: {profession}
Pozisyon: {jobTitle}
Seviye: {level}
Zorluk: {difficulty}
Soru Sayisi: {count}
Dil: {language}

Lutfen bu pozisyon icin hedef dilde ({language}) davranissal mulakat sorulari uret.
Yumusak becerilere, takim calismasina ve catisma cozumune odaklan.{exclusionText}

ZORUNLU ODAK KONULARI VE KILAVUZ:
{matrixGuidance}

KURAL: Lutfen 'Bize kendinden bahset', 'OOP nedir', 'Zayif yonun nedir' gibi klişe, ezberci genel gecer sorulari ASLA SORMA. Mutlaka verilen ODAK KONULARI uzerinden pratik bir durumu (senaryoyu) yasamasi gerekiyormus gibi olay orguleri yarat.

Ciktiyi kesinlikle su JSON formatinda ver:
{{
  ""questions"": [
    {{ ""questionText"": ""Soru metni"", ""codeSnippet"": null }}
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
Soru Sayisi: {count}
Dil: {language}

Lutfen bu meslek grubu ve pozisyon icin hedef dilde ({language}) teknik mulakatta sorulmak uzere {difficulty} zorlugunda sorular uret.
Sorular pozisyona uygun, seviyeye gore ayarlanmis ve secilen dilde olmali. Standart sorulardan ziyade analitik dusunmeyi olc.{exclusionText}

ZORUNLU ODAK KONULARI VE KILAVUZ:
{matrixGuidance}

KURAL: Adayin ezber yetenegini degil, dogrudan ODAK KONULARI baglaminda bir problemi nasil cozecegini soran kurgusal veya teknik VAKALAR/SENARYOLAR sormalisin. Temel tanim sormaktan kacin.
Eger soru bir kod parcasini analiz etmeyi gerektiriyorsa, kodu 'codeSnippet' alanina koy, geri kalan anlatimi 'questionText' alanina koy. Kod gerekmiyorsa 'codeSnippet' null olsun.

Format (JSON):
{{
  ""questions"": [
    {{ ""questionText"": ""Soru metni"", ""codeSnippet"": ""opsiyonel kod blogu"" }}
  ]
}}";
            }
            else if (type == InterviewQuestionType.SystemDesign)
            {
                prompt = $@"Meslek Grubu: {profession}
Pozisyon: {jobTitle}
Seviye: {level}
Zorluk: {difficulty}
Soru Tipi: Teknik Senaryo / Gorsel Akil Yurutme Kod Analizi
Soru Sayisi: {count}
Dil: {language}

Bu soru, mulakatin ozel analiz sorusudur. Lutfen adaya '{profession}' ve '{jobTitle}' metriklerine uygun bir kod parcasi veya sistem mimarisi / algoritma vakasi ver.
Senaryo mantikli ve tutarli bir yazilim/sistem gelistirme problemi olmali.

ZORUNLU ODAK KONULARI VE KILAVUZ:
{matrixGuidance}

KURAL: Siradan sorular kabul edilemez. Tamamen ODAK KONULARI uzerinde muhendislik krizlerini, buyuk refactor intiyaclarini veya derin sistem darboğazlarini barindiran cok ozel bir case olustur.
ONEMLI: Senaryo metnini 'questionText' alanina yaz. Ilgili kodu veya diyagrami ise kesinlikle 'codeSnippet' alanina yaz.
Soru metni icerisinde 'Asagidaki kodu inceleyin' gibi ifadeler kullanarak yonlendirme yap.

Format (JSON):
{{
  ""questions"": [
    {{ ""questionText"": ""Senaryo aciklamasi ve soru"", ""codeSnippet"": ""Sadece kod veya diyagram metni"" }}
  ]
}}";
            }
            else
            {
                return new List<InterviewQuestionResult>();
            }

            var response = await model.GenerateContent(prompt, cancellationToken: cancellationToken);
            var jsonContent = CleanJsonResponse(response?.Text ?? "{}");

            try
            {
                var result = JsonSerializer.Deserialize<InterviewQuestionsRoot>(
                    jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Questions ?? new List<InterviewQuestionResult>();
            }
            catch
            {
                return new List<InterviewQuestionResult>();
            }
        }

        public async Task<FollowUpResult> DecideFollowUpAsync(
            string question,
            string answer,
            string language,
            CancellationToken cancellationToken = default)
        {
            var model = GoogleAI.GenerativeModel(model: ModelName);

            var prompt = $@"Sen bir mulakatcisin. Adayin son cevabina gore mulakatin akisina yardim ediyorsun.
Dil: {language}

Soru: ""{question}""
Adayin Cevabi: ""{answer}""

Gorevin: Sadece cevap cok kapali, net degil veya ek soru gerektiriyorsa requiresFollowUp = true don ve kisa bir takip sorusu yaz.
Normal cevaplarda requiresFollowUp = false don.
Ciktiyi mulakat dilinde ({language}) ver.

Sadece su JSON formatinda yanit ver:
{{
  ""requiresFollowUp"": false,
  ""followUpQuestion"": null
}}";

            var response = await model.GenerateContent(prompt, cancellationToken: cancellationToken);
            var jsonContent = CleanJsonResponse(response?.Text ?? "{}");

            try
            {
                return JsonSerializer.Deserialize<FollowUpResult>(
                    jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new FollowUpResult { RequiresFollowUp = false };
            }
            catch
            {
                return new FollowUpResult { RequiresFollowUp = false };
            }
        }

        public async Task<AssessmentResult> EvaluateInterviewAnswerAsync(
            string question,
            string answer,
            string profession,
            string jobTitle,
            InterviewLevel level,
            DifficultyLevel difficulty,
            string language,
            CancellationToken cancellationToken = default)
        {
            var model = GoogleAI.GenerativeModel(model: ModelName);

            var prompt = $@"Sen {difficulty} zorluk seviyesinde sinav yapan, {profession} alaninda ve {jobTitle} pozisyonu icin uzman bir teknik mulakatcisin ({level} seviye adayi degerlendiriyorsun).
Kullanicinin tercih ettigi dil: {language}. Lutfen tum geri bildirimlerini ve follow-up sorularini bu dilde ver.

Adaya sorulan soru: ""{question}""
Adayin cevabi: ""{answer}""

Cevabi degerlendir:
1. Kisa ve yapici bir geri bildirim ver (maksimum 2 cumle).
2. Cevabi 0 ile 100 arasinda puanla.
3. Sadece cevap kesinlikle kabul edilemez, cok eksik veya tamamen konu disiysa requiresFollowUp = true yap ve bir followUpQuestion yaz.
4. Eger cevap '[SES_ANLASILAMADI]' gibi teknik bir hata yer tutucusu iceriyorsa requiresFollowUp = false olsun ve puani 0 ver.

Sadece su formatta gecerli bir JSON objesi dondur:
{{
  ""score"": 85,
  ""feedback"": ""geri bildirim metni"",
  ""requiresFollowUp"": false,
  ""followUpQuestion"": null
}}";

            var response = await model.GenerateContent(prompt, cancellationToken: cancellationToken);
            var jsonContent = CleanJsonResponse(response?.Text ?? "{}");

            try
            {
                return JsonSerializer.Deserialize<AssessmentResult>(
                    jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new AssessmentResult { Feedback = "Error parsing AI response" };
            }
            catch
            {
                return new AssessmentResult { Feedback = jsonContent };
            }
        }

        public async Task<string> GenerateIdealAnswerSummaryAsync(
            string question,
            string answer,
            string aiFeedback,
            string profession,
            string jobTitle,
            InterviewLevel level,
            DifficultyLevel difficulty,
            string language,
            CancellationToken cancellationToken = default)
        {
            var model = GoogleAI.GenerativeModel(model: ModelName);

            var prompt = $@"Sen uzman bir teknik mulakat kocusun.
Meslek: {profession}
Pozisyon: {jobTitle}
Seviye: {level}
Zorluk: {difficulty}
Dil: {language}

Soru:
{question}

Adayin cevabi:
{answer}

Mevcut AI degerlendirmesi:
{aiFeedback}

Gorevin:
- Bu soruya daha guclu bir cevabin nasil gorunecegini 2-4 cumle ile ozetle.
- Tam bir ideal cevap yazma; sadece yon gosterici, kisa ve uygulanabilir bir ozet ver.
- Metni tamamen {language} dilinde yaz.
- Duz metin disinda baska format kullanma.";

            var response = await model.GenerateContent(prompt, cancellationToken: cancellationToken);
            return response?.Text?.Trim() ?? string.Empty;
        }

        public async Task<string> GenerateFinalFeedbackAsync(
            string profession,
            string jobTitle,
            InterviewLevel level,
            DifficultyLevel difficulty,
            List<InterviewQuestion> questionsAndAnswers,
            string language,
            CancellationToken cancellationToken = default)
        {
            var model = GoogleAI.GenerativeModel(model: ModelName);

            var sb = new StringBuilder();
            sb.AppendLine($"Meslek: {profession}");
            sb.AppendLine($"Pozisyon: {jobTitle}");
            sb.AppendLine($"Seviye: {level}");
            sb.AppendLine($"Zorluk: {difficulty}");
            sb.AppendLine("Mulakat Ozeti");
            sb.AppendLine();
            sb.AppendLine("Iste mulakat transkripti:");

            foreach (var qa in questionsAndAnswers.OrderBy(q => q.Order))
            {
                sb.AppendLine($"- Soru: {qa.QuestionText}");
                sb.AppendLine($"  Cevap: {qa.Answer?.UserAnswerText ?? "[Cevap Yok]"}");
                sb.AppendLine($"  Puan: {qa.Answer?.Score?.ToString() ?? "N/A"}");
            }

            sb.AppendLine();
            sb.AppendLine($"Kapsamli bir mulakat degerlendirme raporu olustur. Guclu yonleri, gelisime acik yonleri ve nihai 'Ise Alim Karari'ni (Olumlu/Olumsuz/Degerlendirilebilir) belirt. Raporu tamamen hedeflenen dilde ({language}) ve Markdown formatinda yaz.");
            sb.AppendLine("ONEMLI: Eger bir cevap '[SES_ANLASILAMADI]', '[SES_HATA]' veya '[CEVAP_YOK]' olarak isaretlenmisse, bunu teknik/kullanici hatasi olarak gor ve 'Cevaplanmadi' kabul et. Bu durum teknik yetkinlik puanini dusurmemeli, sadece verinin eksik oldugu belirtilmeli. Eger sorularin %50'sinden fazlasi cevaplanmadiysa mulakatin tamamlanmadigini belirt.");

            var response = await model.GenerateContent(sb.ToString(), cancellationToken: cancellationToken);
            return response?.Text ?? string.Empty;
        }

        public async Task<SessionAssessmentResult> GenerateSessionAssessmentAsync(
            string profession,
            string jobTitle,
            InterviewLevel level,
            DifficultyLevel difficulty,
            string language,
            List<InterviewQuestion> questionsAndAnswers,
            CancellationToken cancellationToken = default)
        {
            var model = GoogleAI.GenerativeModel(model: ModelName);

            var sb = new StringBuilder();
            sb.AppendLine($"Meslek: {profession}");
            sb.AppendLine($"Pozisyon: {jobTitle}");
            sb.AppendLine($"Seviye: {level}");
            sb.AppendLine($"Zorluk: {difficulty}");
            sb.AppendLine($"Dil: {language}");
            sb.AppendLine();
            sb.AppendLine("Mulakat soru ve cevaplari:");

            foreach (var qa in questionsAndAnswers.OrderBy(q => q.Order))
            {
                sb.AppendLine($"Soru: {qa.QuestionText}");
                sb.AppendLine($"Cevap: {qa.Answer?.UserAnswerText ?? "[Cevap Yok]"}");
                sb.AppendLine($"Metin puani: {qa.Answer?.Score?.ToString() ?? "0"}");
                sb.AppendLine($"AI geri bildirimi: {qa.Answer?.AIAnalysis ?? "[Geri Bildirim Yok]"}");
                sb.AppendLine("---");
            }

            sb.AppendLine();
            sb.AppendLine($@"Sen uzman bir mulakat kocusun. Yukaridaki mulakatin tamamina gore adayin:
1. genel iletisim kalitesini
2. genel ozguven ve sunum kalitesini
degerlendir.

Tum ciktayi {language} dilinde ver.
Yaniti sadece gecerli JSON formatinda ver.

Beklenen format:
{{
  ""communicationScore"": 0,
  ""communicationFeedback"": ""iletisim ile ilgili kisa degerlendirme"",
  ""confidenceScore"": 0,
  ""confidenceFeedback"": ""ozguven ve sunum ile ilgili kisa degerlendirme""
}}");

            var response = await model.GenerateContent(sb.ToString(), cancellationToken: cancellationToken);
            var jsonContent = CleanJsonResponse(response?.Text ?? "{}");

            try
            {
                return JsonSerializer.Deserialize<SessionAssessmentResult>(
                    jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new SessionAssessmentResult();
            }
            catch
            {
                return new SessionAssessmentResult();
            }
        }

        private class InterviewQuestionsRoot
        {
            public List<InterviewQuestionResult> Questions { get; set; } = new();
        }
    }
}
