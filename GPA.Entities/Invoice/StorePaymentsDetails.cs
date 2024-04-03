using GPA.Entities;
using GPA.Entities.Common;

namespace GPA.Common.Entities.Invoice
{
    public class StorePaymentsDetails : Entity<Guid>
    {
        public decimal Payment { get; set; }
        public PaymentStatus Status { get; set; }

        public Guid PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
    }
}
