using GPA.Common.Entities.Inventory;
using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class PurchaseDetails : Entity<Guid>
    {
        public decimal Price { get; set; }

        public Guid? StockId { get; set; }
        public Stock? Stock { get; set; }

        public Guid PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
    }
}
