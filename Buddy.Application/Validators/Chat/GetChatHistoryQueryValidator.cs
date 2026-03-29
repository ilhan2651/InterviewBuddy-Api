using Buddy.Application.Features.Chat.GetHistory;
using FluentValidation;

namespace Buddy.Application.Validators.Chat
{
    public class GetChatHistoryQueryValidator : AbstractValidator<GetChatHistoryQuery>
    {
        public GetChatHistoryQueryValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("SessionId zorunludur.")
                .MaximumLength(100).WithMessage("SessionId en fazla 100 karakter olabilir.");
        }
    }
}
