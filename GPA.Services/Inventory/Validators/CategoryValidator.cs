using FluentValidation;
using GPA.Common.DTOs.Inventory;

namespace GPA.Bussiness.Services.Inventory.Mappers
{
    public class CategoryValidator : AbstractValidator<CategoryDto>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.Name).NotEmpty().NotNull();
            RuleFor(x => x.Description).NotEmpty().NotNull();
        }
    }
}
