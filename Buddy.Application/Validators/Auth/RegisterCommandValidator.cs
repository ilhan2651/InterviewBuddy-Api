using Buddy.Application.Common.Interfaces;
using Buddy.Application.Features.Auth.Register;
using FluentValidation;

namespace Buddy.Application.Validators.Auth
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad soyad zorunludur.")
                .MaximumLength(150).WithMessage("Ad soyad en fazla 150 karakter olabilir.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email zorunludur.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
                .MaximumLength(200).WithMessage("Email en fazla 200 karakter olabilir.")
                .MustAsync(async (email, cancellationToken) =>
                    await unitOfWork.Users.GetByEmailAsync(email.Trim(), cancellationToken) is null)
                .WithMessage("Bu email adresi ile kayıtlı bir kullanıcı zaten var.");
            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));


            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre zorunludur.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .MaximumLength(100).WithMessage("Şifre en fazla 100 karakter olabilir.");
        }
    }
}
