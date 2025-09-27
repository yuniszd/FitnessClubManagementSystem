using FCMS.Application.DTOs.SubscriptionDTOs;
using FluentValidation;

namespace FCMS.Application.Validations.SubscriptionValidations;

public class SubscriptionUpdateDtoValidator : AbstractValidator<SubscriptionUpdateDto>
{
    public SubscriptionUpdateDtoValidator()
    {
        RuleFor(x => x.SubscriptionPlanId).NotEmpty().WithMessage("Plan seçilməlidir.");
        RuleFor(x => x.EndDate).GreaterThan(DateTime.UtcNow).WithMessage("Bitmə tarixi keçmiş ola bilməz.");
        RuleFor(x => x.AllowedVisits)
            .GreaterThan(0).When(x => x.AllowedVisits.HasValue);
    }
}
