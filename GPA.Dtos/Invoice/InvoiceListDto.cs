using GPA.Common.DTOs.Invoice;

namespace GPA.Common.DTOs.Invoices
{
    public class InvoiceListDto
    {
        public Guid? Id { get; set; }
        public byte Status { get; set; }
        public string Code { get; set; }
        public decimal Payment { get; set; }
        public byte PaymentStatus { get; set; }
        public byte PaymentMethod { get; set; }
        public byte Type { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        public Guid ClientId { get; set; }
        public ClientDto Client { get; set; }
        public string? CreatedByName { get; set; }
        public string? UpdatedByName { get; set; }

        public ICollection<InvoiceListDetailDto> InvoiceDetails { get; set; }
    }
}
