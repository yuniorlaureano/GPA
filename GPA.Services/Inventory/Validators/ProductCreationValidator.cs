using FluentValidation;
using GPA.Common.DTOs.Inventory;

namespace GPA.Bussiness.Services.Inventory.Validator
{
    public class ProductCreationValidator : AbstractValidator<ProductCreationDto>
    {
        public ProductCreationValidator()
        {
            RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().NotNull().MaximumLength(50);
            RuleFor(x => x.Price).NotEmpty().NotNull();
            RuleFor(x => x.Description).NotEmpty().NotNull().MaximumLength(300);
        }
    }
}
