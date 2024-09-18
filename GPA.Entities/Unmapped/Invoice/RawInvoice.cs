namespace GPA.Entities.Unmapped.Invoice
{
    public class RawInvoice
    {
        public Guid? Id { get; set; }
        public string Code { get; set; }
        public byte Status { get; set; }
        public decimal Payment { get; set; }
        public byte PaymentStatus { get; set; }
        public byte Type { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        public Guid ClientId { get; set; }
    }
}
