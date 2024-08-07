using GPA.Common.DTOs.Inventory;

namespace GPA.Dtos.Invoice
{
    public class InvoiceFilterDto
    {
        public string? Term { get; set; }
        public DetailedDate? From { get; set; }
        public DetailedDate? To { get; set; }
        public int? Status { get; set; }
        public int? SaleType { get; set; }
    }
}
