using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class StockDetails : Entity<Guid>
    {
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public Guid StockId { get; set; }
        public Stock Stock { get; set; }
    }
}
