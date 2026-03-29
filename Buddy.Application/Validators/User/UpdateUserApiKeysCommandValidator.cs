using Buddy.Application.Features.User.UpdateApiKeys;
using FluentValidation;

namespace Buddy.Application.Validators.User
{
    public class UpdateUserApiKeysCommandValidator : AbstractValidator<UpdateUserApiKeysCommand>
    {
        public UpdateUserApiKeysCommandValidator()
        {
            RuleFor(x => x.SimliApiKey)
                .NotEmpty().WithMessage("Simli API key zorunludur.")
                .MaximumLength(300).WithMessage("Simli API key en fazla 300 karakter olabilir.");

            RuleFor(x => x.ElevenLabsApiKey)
                .NotEmpty().WithMessage("ElevenLabs API key zorunludur.")
                .MaximumLength(300).WithMessage("ElevenLabs API key en fazla 300 karakter olabilir.");
        }
    }
}
