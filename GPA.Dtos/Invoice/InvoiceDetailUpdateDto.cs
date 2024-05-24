namespace GPA.Common.DTOs.Invoices
{
    public class InvoiceDetailUpdateDto
    {
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
    }
}