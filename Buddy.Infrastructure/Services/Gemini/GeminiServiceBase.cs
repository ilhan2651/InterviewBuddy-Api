using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using System;

namespace Buddy.Infrastructure.Services.Gemini
{
    public abstract class GeminiServiceBase
    {
        protected readonly GoogleAI GoogleAI;
        protected readonly string ModelName;

        protected GeminiServiceBase(IConfiguration configuration)
        {
            var apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured");
            var configuredModel = configuration["Gemini:ModelName"] ?? "gemini-flash-latest";

            ModelName = configuredModel;
            GoogleAI = new GoogleAI(apiKey: apiKey);
        }

        protected string CleanJsonResponse(string jsonContent)
        {
            if (string.IsNullOrEmpty(jsonContent))
            {
                return "{}";
            }

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
    }
}
