namespace GPA.Common.Entities.Invoice
{
    public class ClientPaymentsDetailSummary
    {
        public Guid InvoiceId { get; set; }
        public string Client { get; set; }
        public decimal PendingPayment { get; set; }
        public decimal Payment { get; set; }
    }
}
