namespace GPA.Common.DTOs.Invoice
{
    public class InvoiceWithReceivableAccountsDto
    {
        public Guid InvoiceId { get; set; }
        public int InvoiceStatus { get; set; }
        public int SaleType { get; set; }
        public DateTime Date { get; set; }
        public string InvoiceNote { get; set; }
        public int PaymentStatus { get; set; }
        public decimal Payment { get; set; }
        public string ClientName { get; set; }
        public string ClientIdentification { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }
        public ClientPaymentsDetailDto? PendingPayment { get; set; }
        public IEnumerable<ClientPaymentsDetailDto>? ReceivableAccounts { get; set; }
    }
}
