namespace GPA.Common.DTOs.Invoice
{
    public class ClientDebitDto
    {
        public Guid Id { get; set; }
        public decimal PendingPayment { get; set; }
        public decimal Payment { get; set; }
        public Guid InvoiceId { get; set; }
    }
}
