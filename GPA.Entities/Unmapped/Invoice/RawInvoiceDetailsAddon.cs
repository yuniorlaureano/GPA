namespace GPA.Entities.Unmapped.Invoice
{
    public class RawInvoiceDetailsAddon
    {
        public Guid Id { get; set; }
        public string Concept { get; set; }
        public bool IsDiscount { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public Guid InvoiceDetailId { get; set; }
    }
}
