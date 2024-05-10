using GPA.Common.DTOs.Unmapped;

namespace GPA.Common.DTOs.Inventory
{
    public class StockDetailsDto
    {
        public Guid? Id { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
        public ProductDto Product { get; set; }
        public Guid StockId { get; set; }
    }
}
