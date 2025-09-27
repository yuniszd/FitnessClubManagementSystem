using FCMS.Application.DTOs.MemberDTOs;
using FluentValidation;

namespace FCMS.Application.Validations.MemberValidations;

public class CreateMemberDtoValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MaximumLength(100).WithMessage("Ad 100 simvoldan uzun ola bilməz.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email düzgün formatda deyil.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{10,15}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Telefon nömrəsi düzgün formatda deyil.");

        RuleFor(x => x.SubscriptionPlanId)
            .NotEmpty().WithMessage("Subscription plan seçilməlidir.");

        RuleFor(x => x.AllowedVisits)
            .GreaterThan(0).When(x => x.AllowedVisits.HasValue)
            .WithMessage("AllowedVisits sıfırdan böyük olmalıdır.");
    }
}

