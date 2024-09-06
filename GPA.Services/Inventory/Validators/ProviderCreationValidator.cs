using FluentValidation;
using GPA.Common.DTOs.Inventory;

namespace GPA.Services.Invoice.Validators
{
    public class ProviderCreationValidator : AbstractValidator<ProviderDto>
    {
        public ProviderCreationValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .NotNull().WithMessage("El nombre es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("El teléfono es requerido.")
                .NotNull().WithMessage("El teléfono es requerido.")
                .MaximumLength(15).WithMessage("El teléfono no puede exceder los 15 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .NotNull().WithMessage("El email es requerido.")
                .MaximumLength(254).WithMessage("El email no puede exceder los 254 caracteres.");

            RuleFor(x => x.Street)
                .MaximumLength(100).WithMessage("El email no puede exceder los 100 caracteres.");

            RuleFor(x => x.BuildingNumber)
                .MaximumLength(10).WithMessage("El número de local no puede exceder los 10 caracteres.");

            RuleFor(x => x.Identification)
                .MaximumLength(20).WithMessage("El número de identificación no puede exceder los 20 caracteres.");

            RuleFor(x => x.City)
                .MaximumLength(50).WithMessage("La ciudad no puede exceder los 50 caracteres.");

            RuleFor(x => x.State)
                .MaximumLength(50).WithMessage("La estado no puede exceder los 50 caracteres.");

            RuleFor(x => x.Country)
                .MaximumLength(50).WithMessage("El país no puede exceder los 50 caracteres.");

            RuleFor(x => x.PostalCode)
                .MaximumLength(50).WithMessage("El código postal no puede exceder los 50 caracteres.");

        }
    }
}
