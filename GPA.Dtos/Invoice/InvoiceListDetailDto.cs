using GPA.Common.DTOs.Inventory;
using GPA.Common.DTOs.Unmapped;

namespace GPA.Common.DTOs.Invoices
{
    public class InvoiceListDetailDto
    {
        public Guid? Id { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }

        public ProductDto Product { get; set; }
        public ProductCatalogDto StockProduct { get; set; }
    }
}