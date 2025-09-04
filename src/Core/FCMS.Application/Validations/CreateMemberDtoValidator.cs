// Validators/CreateMemberDtoValidator.cs
using FCMS.Application.DTOs.MemberDTOs;
using FluentValidation;

public class CreateMemberDtoValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad boş ola bilməz")
            .Length(2, 50).WithMessage("Ad 2-50 simvol arası olmalıdır");
    }
}
