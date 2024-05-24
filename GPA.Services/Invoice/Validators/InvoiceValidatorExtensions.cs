using FluentValidation;
using GPA.Common.DTOs.Invoices;
using GPA.Services.Invoice.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Bussiness.Services.Invoice.Validator
{
    public static class InvoiceValidatorExtensions
    {
        public static void AddInvoiceValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<InvoiceUpdateDto>, InvoiceUpdateValidator>();
            services.AddScoped<IValidator<InvoiceDto>, InvoiceValidator>();
        }
    }
}
