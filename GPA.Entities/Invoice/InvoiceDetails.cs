using GPA.Common.Entities.Inventory;

namespace GPA.Common.Entities.Invoice
{
    public class InvoiceDetails
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
    }
}
