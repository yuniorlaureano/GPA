using GPA.Entities.Unmapped.Invoice;

namespace GPA.Entities.Unmapped
{
    public class InvoicePrintDetails
    {
        public RawInvoiceDetails RawInvoiceDetails { get; set; }
        public List<RawInvoiceDetailsAddon> RawInvoiceDetailsAddon { get; set; }
    }
}
