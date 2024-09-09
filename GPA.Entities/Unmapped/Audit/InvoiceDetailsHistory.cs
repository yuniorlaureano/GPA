namespace GPA.Entities.Unmapped.Audit
{
    public class InvoiceDetailsHistory
    {
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
    }
}
