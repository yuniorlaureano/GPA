using GPA.Common.Entities.Inventory;
using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class InvoiceDetails : Entity<Guid>
    {
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid? ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
