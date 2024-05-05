using GPA.Business.Services.Invoice;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Business.Invoice.Extensions
{
    public static class BusinessInvoiceExtensions
    {
        public static void AddBusinessInvoiceServices(this IServiceCollection services)
        {
            services.AddTransient<IClientService, ClientService>();
            services.AddTransient<IInvoiceService, InvoiceService>();
        }
    }
}
