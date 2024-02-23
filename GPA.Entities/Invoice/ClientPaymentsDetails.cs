using GPA.Entities;
using GPA.Entities.Common;

namespace GPA.Common.Entities.Invoice
{
     public class ClientPaymentsDetails : Entity
    {
        public decimal Payment { get; set; }
        public PaymentStatus Status { get; set; }

        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
    }
}
