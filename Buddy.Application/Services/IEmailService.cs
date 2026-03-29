using System;
using System.Collections.Generic;
using System.Text;

namespace Buddy.Application.Services
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(
            string toEmail,
            string fullName,
            string verificationLink,
            CancellationToken cancellationToken = default
            );

    }
}
