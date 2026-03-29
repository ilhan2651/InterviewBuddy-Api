using Buddy.Application.Features.Chat.SendTextMessage;
using FluentValidation;

namespace Buddy.Application.Validators.Chat
{
    public class SendTextMessageCommandValidator : AbstractValidator<SendTextMessageCommand>
    {
        public SendTextMessageCommandValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Mesaj boş olamaz.")
                .MaximumLength(4000).WithMessage("Mesaj en fazla 4000 karakter olabilir.");

            RuleFor(x => x.SessionId)
                .MaximumLength(100).WithMessage("SessionId en fazla 100 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.SessionId));

            RuleFor(x => x.AnonymousId)
                .MaximumLength(100).WithMessage("AnonymousId en fazla 100 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.AnonymousId));
        }
    }
}
