namespace GPA.Common.DTOs.Invoice
{
    public class ClientPaymentsDetailSummaryDto
    {
        public Guid InvoiceId { get; set; }
        public string Client { get; set; }
        public decimal PendingPayment { get; set; }
        public decimal Payment { get; set; }
    }
}
