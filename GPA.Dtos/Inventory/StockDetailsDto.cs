using GPA.Common.DTOs.Unmapped;

namespace GPA.Common.DTOs.Inventory
{
    public class StockDetailsDto
    {
        public Guid? Id { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductCategoryId { get; set; }
        public Guid StockId { get; set; }

        public RawProductCatalogDto StockProduct { get; set; }
    }
}
