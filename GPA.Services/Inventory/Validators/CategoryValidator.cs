using FluentValidation;
using GPA.Common.DTOs.Inventory;

namespace GPA.Bussiness.Services.Inventory.Validator
{
    public class CategoryValidator : AbstractValidator<CategoryDto>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido")
                .NotNull().WithMessage("El nombre es requerido")
                .MaximumLength(50).WithMessage("El nombre solo admite 50 caracteres");
            
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es requerido")
                .NotNull().WithMessage("La descripción es requerido")
                .MaximumLength(200).WithMessage("La descripción solo admite 200 caracteres");
        }
    }
}
