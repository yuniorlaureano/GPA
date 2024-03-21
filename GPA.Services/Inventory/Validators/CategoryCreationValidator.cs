using FluentValidation;
using GPA.Common.DTOs.Inventory;

namespace GPA.Bussiness.Services.Inventory.Validator
{
    public class CategoryCreationValidator : AbstractValidator<CategoryDto>
    {
        public CategoryCreationValidator()
        {
            RuleFor(x => x.Name).NotEmpty().NotNull();
            RuleFor(x => x.Description).NotEmpty().NotNull();
        }
    }
}
