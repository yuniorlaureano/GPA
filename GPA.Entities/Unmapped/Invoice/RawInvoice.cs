namespace GPA.Entities.Unmapped.Invoice
{
    public class RawInvoice
    {
        public Guid? Id { get; set; }
        public int Status { get; set; }
        public decimal Payment { get; set; }
        public int Type { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        public Guid ClientId { get; set; }
    }
}
