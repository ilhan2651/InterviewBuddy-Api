using System;
using System.Collections.Generic;
using System.Text;

namespace Buddy.Application.Features.Auth.VerifyEmail
{
    public class VerifyEmailResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
