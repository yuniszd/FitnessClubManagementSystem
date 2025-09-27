using FCMS.Application.DTOs.SubscriptionPlanDTOs;
using FluentValidation;

namespace FCMS.Application.Validations.SubscriptionPlanValidations;

public class SubscriptionPlanCreateDtoValidator : AbstractValidator<SubscriptionPlanCreateDto>
{
    public SubscriptionPlanCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan adı boş ola bilməz.")
            .MaximumLength(50).WithMessage("Plan adı 50 simvoldan uzun ola bilməz.");

        RuleFor(x => x.DurationInMonths)
            .GreaterThan(0).WithMessage("Müddət minimum 1 ay olmalıdır.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Qiymət sıfırdan böyük olmalıdır.");
    }
}
