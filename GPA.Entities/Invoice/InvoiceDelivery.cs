using GPA.Common.Entities.Comon;
using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class InvoiceDelivery : Entity
    {
        public Guid InvoiceId { get; set; }
        public Guid DeliveryId { get; set; }

        public Invoice Invoice { get; set; }
        public Delivery Delivery { get; set; }
    }
}
