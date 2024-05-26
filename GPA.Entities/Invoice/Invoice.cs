using GPA.Entities;
using GPA.Entities.Common;

namespace GPA.Common.Entities.Invoice
{
    public class Invoice : Entity<Guid>
    {
        public SaleType Type { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal Payment { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        public Guid ClientId { get; set; }
        public required Client Client { get; set; }

        public required ICollection<InvoiceDetails> InvoiceDetails { get; set; }
        public ICollection<InvoiceDelivery>? InvoiceDeliveries { get; set; }
        public ICollection<ClientPaymentsDetails>? ClientPaymentsDetails { get; set; }
    }
}
