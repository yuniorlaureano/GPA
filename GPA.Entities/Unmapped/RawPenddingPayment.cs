namespace GPA.Entities.Unmapped
{
    public class RawPenddingPayment
    {
        public Guid Id { get; set; }
        public decimal PendingPayment { get; set; }
        public decimal Payment { get; set; }
        public Guid InvoiceId { get; set; }
    }
}
