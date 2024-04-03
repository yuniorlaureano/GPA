using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class Invoice : Entity<Guid>
    {
        public Guid SellId { get; set; }
        public Sell Sell { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public ICollection<ClientPaymentsDetails> ClientPaymentsDetails { get; set; }
        public ICollection<InvoiceDelivery> InvoiceDeliveries { get; set; }
    }
}
