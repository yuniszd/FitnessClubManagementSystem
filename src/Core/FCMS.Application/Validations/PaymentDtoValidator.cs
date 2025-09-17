using FCMS.Application.DTOs.PaymentDTOs;
using FluentValidation;

namespace FCMS.Application.Validations;

public class PaymentDtoValidator : AbstractValidator<PaymentDto>
{
    public PaymentDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id boş ola bilməz.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("SubscriptionId boş ola bilməz.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Ödəniş məbləği sıfırdan böyük olmalıdır.");

        RuleFor(x => x.PaidDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Ödəniş tarixi gələcək tarix ola bilməz.");
    }
}