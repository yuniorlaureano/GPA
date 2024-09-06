using FluentValidation;
using GPA.Common.DTOs.General;

namespace GPA.Services.General.Validators
{
    public class UnitValidator : AbstractValidator<UnitDto>
    {
        public UnitValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("El campo Código es requerido.")
                .NotNull().WithMessage("El campo Código es requerido.")
                .MaximumLength(10).WithMessage("El campo Código no puede tener más de 10 caracteres.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El campo Nombre es requerido.")
                .NotNull().WithMessage("El campo Nombre es requerido.")
                .MaximumLength(50).WithMessage("El campo Nombre no puede tener más de 50 caracteres.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("El campo Descripción es requerido.")
                .NotNull().WithMessage("El campo Descripción es requerido.")
                .MaximumLength(200).WithMessage("El campo Descripción no puede tener más de 200 caracteres.");
        }
    }
}
