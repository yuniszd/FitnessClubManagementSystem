using FCMS.Application.DTOs.PaymentDTOs;
using FluentValidation;

namespace FCMS.Application.Validations.PaymentValidations;

public class PaymentCreateDtoValidator : AbstractValidator<PaymentCreateDto>
{
    public PaymentCreateDtoValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("SubscriptionId boş ola bilməz.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Ödəniş məbləği sıfırdan böyük olmalıdır.");
    }
}
