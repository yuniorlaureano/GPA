namespace GPA.Entities.Unmapped.Invoice
{
    public class RawInvoiceDetails
    {
        public Guid? Id { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
    }
}
