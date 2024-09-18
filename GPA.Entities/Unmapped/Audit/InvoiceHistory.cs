using GPA.Entities.General;

namespace GPA.Entities.Unmapped.Audit
{
    public class InvoiceHistory : EntityHistory<Guid>
    {
        public SaleType Type { get; set; }
        public string Code { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal Payment { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        public Guid ClientId { get; set; }
        public string InvoiceDetailsHistory { get; set; }
    }
}
