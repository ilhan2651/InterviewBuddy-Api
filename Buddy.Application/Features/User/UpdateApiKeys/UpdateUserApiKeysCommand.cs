using MediatR;

namespace Buddy.Application.Features.User.UpdateApiKeys
{
    public class UpdateUserApiKeysCommand : IRequest<bool>
    {
        public string SimliApiKey { get; set; } = string.Empty;
        public string ElevenLabsApiKey { get; set; } = string.Empty;
    }
}
