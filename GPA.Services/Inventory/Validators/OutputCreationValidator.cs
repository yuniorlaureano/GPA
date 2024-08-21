using FluentValidation;
using GPA.Common.DTOs.Inventory;
using GPA.Entities.General;

namespace GPA.Bussiness.Services.Inventory.Validator
{
    public class OutputCreationValidator : AbstractValidator<OutputCreationDto>
    {
        public OutputCreationValidator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(300).WithMessage("La descripción solo admite 300 caracteres.");

            RuleFor(x => x.TransactionType)
                .Must(x => x == 1).WithMessage("El tipo de transacción debe ser '1', que equivale a salida.");

            RuleFor(x => x.Status)
                .Must(x => Enum.GetValues<StockStatus>().Any(e => ((int)e) == x)).WithMessage("El status debe ser Borrador=0, Guardado=1, Cancelado=2");

            RuleFor(x => x.ReasonId)
                .Must(x => x == 5 || x == 6 || x == 7).WithMessage("La razon debe ser: 'Producto defectuoso'=5, 'Producto expirado'=6, 'Materia prima'=7");

            RuleFor(x => x.StockDetails)
                .Must(x => x.All(x => x.ProductId != Guid.Empty && x.Quantity > 0)).WithMessage("No puede dar salida a 0 productos."); ;
        }
    }
}
