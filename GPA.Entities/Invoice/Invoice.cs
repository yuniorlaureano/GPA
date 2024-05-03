using GPA.Entities;
using GPA.Entities.Common;

namespace GPA.Common.Entities.Invoice
{
    public class Invoice : Entity<Guid>
    {
        public string Status { get; set; }
        public SaleType Type { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? Note { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public ICollection<ClientPaymentsDetails> ClientPaymentsDetails { get; set; }
        public ICollection<InvoiceDelivery> InvoiceDeliveries { get; set; }
    }
}
