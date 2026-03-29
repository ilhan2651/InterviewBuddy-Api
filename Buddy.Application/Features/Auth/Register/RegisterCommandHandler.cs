using BCrypt.Net;
using Buddy.Application.Common.Interfaces;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<RegisterCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var verificationToken = Guid.NewGuid().ToString("N");

            var user = new Buddy.Domain.Entities.User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsEmailVerified = false,
                EmailVerificationToken= verificationToken,
                EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24)

            };

            await _unitOfWork.GetRepository<Buddy.Domain.Entities.User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var verificationLink = $"{frontendBaseUrl}/verify-email?token={verificationToken}";

            try
            {
                await _emailService.SendEmailVerificationAsync(
                    user.Email,
                    user.FullName,
                    verificationLink,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Verification email could not be sent for user {Email}", user.Email);

                return new RegisterResponse
                {
                    Success = true,
                    Message = "Kayit basarili ancak dogrulama maili gonderilemedi.",
                    UserId = user.Id
                };
            }

            return new RegisterResponse
            {
                Success = true,
                Message = "Kayit basarili. Lutfen email adresinizi dogrulayin.",
                UserId = user.Id
            };
        }
    }
}
