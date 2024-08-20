using FluentValidation;
using GPA.Common.DTOs.Inventory;

namespace GPA.Bussiness.Services.Inventory.Validator
{
    public class ProductCreationValidator : AbstractValidator<ProductCreationDto>
    {
        public ProductCreationValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .NotNull().WithMessage("El nombre es requerido.")
                .MaximumLength(200).WithMessage("El nombre admite máximo 200 caracteres.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("El código del producto es requerido.")
                .NotNull().WithMessage("El código del producto es requerido.")
                .MaximumLength(50).WithMessage("El código admite máximo 50 caracteres.");

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("El precio es requerido.")
                .NotNull().WithMessage("El precio es requerido.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es requerido.")
                .NotNull().WithMessage("La descripción es requerido.")
                .MaximumLength(300).WithMessage("La descripción admite máximo 300 caracteres.");
        }
    }
}
