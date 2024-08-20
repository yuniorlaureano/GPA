using FluentValidation;
using GPA.Common.DTOs.Inventory;

namespace GPA.Bussiness.Services.Inventory.Validator
{
    public class AddonValidator : AbstractValidator<AddonDto>
    {
        public AddonValidator()
        {
            RuleFor(x => x.Concept)
                .NotEmpty().WithMessage("El concepto requerido.")
                .NotNull().WithMessage("El concepto requerido.")
                .MaximumLength(50).WithMessage("El concepto solo admite 50 caracteres");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("El valor requerido.")
                .NotNull().WithMessage("El valor es requerido.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("El tipo requerido.")
                .NotNull().WithMessage("El tipo es requerido.");
        }
    }
}
