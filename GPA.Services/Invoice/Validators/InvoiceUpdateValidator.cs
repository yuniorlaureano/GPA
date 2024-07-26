using FluentValidation;
using GPA.Common.DTOs.Invoices;
using GPA.Data.Invoice;
using GPA.Entities.General;

namespace GPA.Services.Invoice.Validators
{
    public class InvoiceUpdateValidator : AbstractValidator<InvoiceUpdateDto>
    {
        public InvoiceUpdateValidator(IInvoiceRepository invoiceRepository, IClientRepository clientRepository)
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .NotNull()
                .MustAsync(async (id, token) =>
                {
                    Common.Entities.Invoice.Invoice? invoice = await invoiceRepository.GetByIdAsync(
                        query => query,
                        x => x.Id == id);

                    return invoice?.Status == InvoiceStatus.Draft;
                }).WithMessage("No puede modificar la factura, está en estus (Guardado)\n" +
                               "Cancele la factura y cree otra");

            RuleFor(x => x.InvoiceDetails)
                .Must(details => details.Any())
                .WithMessage("Debe seleccionar al menos un producto");

            RuleFor(x => x.Status)
                .Must(status =>
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
