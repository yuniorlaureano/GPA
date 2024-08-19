using FluentValidation;
using GPA.Common.DTOs.Inventory;

namespace GPA.Bussiness.Services.Inventory.Validator
{
    public class StockCycleCreationValidator : AbstractValidator<StockCycleCreationDto>
    {
        public StockCycleCreationValidator()
        {
            RuleFor(x => x.StartDate).NotEmpty().NotNull();
            RuleFor(x => x.EndDate).NotEmpty().NotNull();
            RuleFor(x => x.Note).NotEmpty().NotNull();
        }
    }
}
