using Buddy.Application.Common.Interfaces;
using Buddy.Application.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Buddy.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SimliController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SimliController> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;
        private readonly IApiKeyValidationService _apiKeyValidationService;

        public SimliController(IConfiguration configuration, ILogger<SimliController> logger, 
            ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IEncryptionService encryptionService,
            IApiKeyValidationService apiKeyValidationService)
        {
            _configuration = configuration;
            _logger = logger;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _apiKeyValidationService = apiKeyValidationService;
        }

        [HttpGet("config")]
        public async Task<ActionResult<SimliConfigResponse>> GetConfig()
        {
            _logger.LogInformation("Simli config requested by user {UserId}", User.Identity?.Name);

            var apiKey = _configuration["Simli:ApiKey"];
            var faceId = _configuration["Simli:FaceId"];

            // --- Check Quota ---
            var currentUserIntId = _currentUserService.UserId;
            if (currentUserIntId.HasValue)
            {
                var userId = currentUserIntId.Value;
                var user = await _unitOfWork.Users.GetQueryable()
                    .Include(u => u.InterviewSessions)
                    .Include(u => u.ApiKeys)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user != null)
                {
                    var completedInterviewCount = user.InterviewSessions.Count(s => s.CompletedAt.HasValue);
                    bool hasFreeQuota = completedInterviewCount == 0;
                    if (!hasFreeQuota)
                    {
                        if (user.ApiKeys == null || string.IsNullOrEmpty(user.ApiKeys.SimliApiKey))
                        {
                            return BadRequest(new { message = "Ücretsiz mülakat hakkınız doldu. Lütfen 'Ayarlar' sayfasından kendi Simli API Anahtarınızı sisteme girin.", code = "QUOTA_EXCEEDED" });
                        }
                        
                        // Use user's decrypted key instead
                        apiKey = _encryptionService.Decrypt(user.ApiKeys.SimliApiKey);

                        var (isValid, errorMessage) = await _apiKeyValidationService.ValidateSimliKeyAsync(apiKey);
                        if (!isValid)
                        {
                            return BadRequest(new
                            {
                                message = errorMessage ?? "Kaydettiğiniz Simli API anahtarı geçersiz görünüyor. Lütfen anahtarınızı güncelleyin.",
                                code = "INVALID_USER_API_KEY"
                            });
                        }
                    }
                }
            }
            // -------------------

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(faceId))
            {
                _logger.LogError("Simli configuration missing. API Key present: {ApiKeyPresent}, Face ID present: {FaceIdPresent}", 
                    !string.IsNullOrEmpty(apiKey), !string.IsNullOrEmpty(faceId));
                return NotFound("Simli configuration not found.");
            }

            _logger.LogInformation("Simli configuration found and returned successfully.");

            return Ok(new SimliConfigResponse
            {
                ApiKey = apiKey,
                FaceId = faceId
            });
        }
    }

    public class SimliConfigResponse
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }

        [JsonPropertyName("faceId")]
        public string FaceId { get; set; }
    }
}
