using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buddy.Application.Features.Auth.VerifyEmail
{
    public class VerifyEmailCommand : IRequest<VerifyEmailResponse>
    {
        public string Token { get; set; } = string.Empty;

    }
}
