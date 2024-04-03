using GPA.Common.Entities.Inventory;
using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class SellDetails : Entity<Guid>
    {
        public decimal Price { get; set; }

        public Guid? StockId { get; set; }
        public Stock? Stock { get; set; }

        public Guid SellId { get; set; }
        public Sell Sell { get; set; }
    }
}
