using GPA.Data.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Data.Invoice.Extensions
{
    public static class DataInvoiceExtensions
    {
        public static void AddDataInvoiceRepositories(this IServiceCollection services)
        {
            services.AddTransient<IClientRepository, ClientRepository>();
            services.AddTransient<IInvoiceRepository, InvoiceRepository>();
            services.AddTransient<IReceivableAccountRepository, ReceivableAccountRepository>();
            services.AddTransient<IInvoiceAttachmentRepository, InvoiceAttachmentRepository>();
            services.AddTransient<IInvoicePrintRepository, InvoicePrintRepository>();
        }
    }
}
