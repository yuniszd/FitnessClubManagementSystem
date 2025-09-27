using FCMS.Application.DTOs.SubscriptionDTOs;
using FluentValidation;

namespace FCMS.Application.Validations.SubscriptionValidations;

public class SubscriptionCreateDtoValidator : AbstractValidator<SubscriptionCreateDto>
{
    public SubscriptionCreateDtoValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithMessage("Üzv seçilməlidir.");
        RuleFor(x => x.SubscriptionPlanId).NotEmpty().WithMessage("Plan seçilməlidir.");
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Başlama tarixi vacibdir.");
        RuleFor(x => x.AllowedVisits)
            .GreaterThan(0).When(x => x.AllowedVisits.HasValue);
    }
}

