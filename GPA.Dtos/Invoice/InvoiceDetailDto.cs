namespace GPA.Common.DTOs.Invoices
{
    public class InvoiceDetailDto
    {
        public Guid? Id { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
    }
}