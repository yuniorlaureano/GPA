using FluentValidation;
using GPA.Common.DTOs.Inventory;
using GPA.Entities.General;

namespace GPA.Bussiness.Services.Inventory.Validator
{
    public class StockCreationValidator : AbstractValidator<StockCreationDto>
    {
        public StockCreationValidator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(300).WithMessage("La descripción solo admite 300 caracteres.");

            RuleFor(x => x.TransactionType)
                .Must(x => x > 0).WithMessage("El tipo de transacción es requerido.")
                .Must(x => x == 2).WithMessage("El tipo de transacción debe ser '2', que equivale a entrada.");

            RuleFor(x => x.Status)
                .Must(x => Enum.GetValues<StockStatus>().Any(e => ((int)e) == x)).WithMessage("El status debe ser Borrador=0, Guardado=1, Cancelado=2");

            RuleFor(x => x.ReasonId)
                .Must(x => x == 1 || x == 4).WithMessage("La razon debe ser: 'Compra'=1, 'Manufacturado'=4");

            RuleFor(x => x.StockDetails)
                .Must(x => x.All(x => x.ProductId != Guid.Empty && x.Quantity > 0)).WithMessage("No puede dar entrada 0 productos.");
        }
    }
}
