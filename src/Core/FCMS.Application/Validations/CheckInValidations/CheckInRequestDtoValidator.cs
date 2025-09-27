using FCMS.Application.DTOs.CheckInDTOs;
using FluentValidation;

namespace FCMS.Application.Validations.CheckInValidations;

public class CheckInRequestDtoValidator : AbstractValidator<CheckInRequestDto>
{
    public CheckInRequestDtoValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Kart nömrəsi boş ola bilməz.");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("DeviceId boş ola bilməz.");
    }
}
