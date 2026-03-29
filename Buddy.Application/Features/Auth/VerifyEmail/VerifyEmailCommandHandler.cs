using Buddy.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buddy.Application.Features.Auth.VerifyEmail
{
    public class VerifyEmailCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
    {
        public async Task<VerifyEmailResponse> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await unitOfWork.Users.GetByEmailVerificationTokenAsync(request.Token, cancellationToken);

            if (user == null)
            {
                return new VerifyEmailResponse
                {
                    Success = false,
                    Message = "Geçersiz doğrulama linki."
                };

            }
            if (user.EmailVerificationTokenExpiresAt.HasValue &&
               user.EmailVerificationTokenExpiresAt.Value < DateTime.UtcNow)
            {
                return new VerifyEmailResponse
                {
                    Success = false,
                    Message = "Doğrulama linkinin süresi dolmuş."
                };
            }
            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;

            await unitOfWork.SaveChangesAsync();

            return new VerifyEmailResponse
            {
                Success = true,
                Message = "E-posta başarıyla doğrulandı."
            };
        }
    }
}
