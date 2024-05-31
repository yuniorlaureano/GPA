using GPA.Common.DTOs.Inventory;

namespace GPA.Common.DTOs.Invoice
{
    public class ClientPaymentsDetailDto
    {
        public Guid? Id { get; set; }
        public decimal PendingPayment { get; set; }
        public decimal Payment { get; set; }
        public DetailedDate Date { get; set; }
        public Guid InvoiceId { get; set; }
    }
}
