namespace GPA.Common.Entities.Invoice
{
    public class InvoiceDetailsAddon
    {
        public Guid Id { get; set; }
        public string Concept { get; set; }
        public bool IsDiscount { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }

        public Guid InvoiceDetailId { get; set; }
        public InvoiceDetails InvoiceDetails { get; set; }
    }
}
