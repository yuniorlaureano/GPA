using FluentValidation;
using GPA.Common.DTOs.Inventory;

namespace GPA.Bussiness.Services.Inventory.Validator
{
    public class CategoryCreationValidator : AbstractValidator<CategoryDto>
    {
        public CategoryCreationValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El campo Nombre es requerido.")
                .NotNull().WithMessage("El campo Nombre es requerido.");
            
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("El campo Descripción es requerido.")
                .NotNull().WithMessage("El campo Descripción es requerido.");
        }
    }
}
