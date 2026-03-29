using Buddy.Application.Features.Chat.SendAudioMessage;
using FluentValidation;

namespace Buddy.Application.Validators.Chat
{
    public class SendAudioMessageCommandValidator : AbstractValidator<SendAudioMessageCommand>
    {
        public SendAudioMessageCommandValidator()
        {
            RuleFor(x => x.AudioStream)
                .NotNull().WithMessage("Ses dosyası zorunludur.");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("Dosya adı zorunludur.")
                .MaximumLength(255).WithMessage("Dosya adı en fazla 255 karakter olabilir.");

            RuleFor(x => x.SessionId)
                .MaximumLength(100).WithMessage("SessionId en fazla 100 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.SessionId));

            RuleFor(x => x.AnonymousId)
                .MaximumLength(100).WithMessage("AnonymousId en fazla 100 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.AnonymousId));
        }
    }
}
