using FluentValidation;
using GPA.Common.DTOs.Invoices;
using GPA.Data.Invoice;
using GPA.Entities.General;

namespace GPA.Services.Invoice.Validators
{
    public class InvoiceValidator : AbstractValidator<InvoiceDto>
    {
        public InvoiceValidator(IClientRepository clientRepository)
        {
            RuleFor(x => x.InvoiceDetails)
                .Must(details => details.Any())
                .WithMessage("Debe seleccionar al menos un producto");

            RuleFor(x => x.Status)
                .Must((status) =>
                {
                    return Enum.TryParse(typeof(InvoiceStatus), status.ToString(), out var _);
                });

            RuleFor(x => x.ClientId)
                .NotEmpty()
                .NotNull()
                .WithMessage("El cliente es requerido")
                .MustAsync(async (clientId, token) =>
                {
                    var cient = await clientRepository.GetByIdAsync(
                            query => query,
                            x => x.Id == clientId
                        );
                    return cient is not null;
                }).WithMessage("El cliente no existe");
        }
    }
}
