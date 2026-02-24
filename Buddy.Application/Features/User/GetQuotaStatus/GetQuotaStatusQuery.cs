using MediatR;

namespace Buddy.Application.Features.User.GetQuotaStatus
{
    public class GetQuotaStatusQuery : IRequest<QuotaStatusResponse>
    {
    }

    public class QuotaStatusResponse
    {
        public bool HasFreeQuota { get; set; }
        public bool HasOwnKeys { get; set; }
    }
}
