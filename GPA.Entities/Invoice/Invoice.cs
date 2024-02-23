using GPA.Common.Entities.Inventory;
using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class Invoice : Entity
    {
        public Guid SellId { get; set; }
        public Sell Sell { get; set; }

        public Guid ClientId { get; set; }
        public Sell Client { get; set; }

        public Guid DeliveryId { get; set; }
        public Delivery Delivery { get; set; }
    }
}
