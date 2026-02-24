using Buddy.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.User.GetQuotaStatus
{
    public class GetQuotaStatusQueryHandler : IRequestHandler<GetQuotaStatusQuery, QuotaStatusResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetQuotaStatusQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<QuotaStatusResponse> Handle(GetQuotaStatusQuery request, CancellationToken cancellationToken)
        {
            var currentUserIntId = _currentUserService.UserId;
            if (!currentUserIntId.HasValue)
            {
                throw new UnauthorizedAccessException("Giriş yapan kullanıcı bulunamadı.");
            }
            var userId = currentUserIntId.Value;

            var user = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.InterviewSessions)
                .Include(u => u.ApiKeys)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");
            }

            bool hasFreeQuota = (userId == 2) || (user.InterviewSessions.Count == 0);
            bool hasOwnKeys = user.ApiKeys != null && 
                              !string.IsNullOrEmpty(user.ApiKeys.SimliApiKey) && 
                              !string.IsNullOrEmpty(user.ApiKeys.ElevenLabsApiKey);

            return new QuotaStatusResponse
            {
                HasFreeQuota = hasFreeQuota,
                HasOwnKeys = hasOwnKeys
            };
        }
    }
}
